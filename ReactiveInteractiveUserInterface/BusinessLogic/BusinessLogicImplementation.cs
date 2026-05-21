//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    /// <summary>
    /// Implementacja warstwy logiki.
    /// Odpowiada za detekcję i obsługę kolizji kul ze ścianami i innymi kulami.
    /// Sekcje krytyczne chronią współdzielone dane przed wyścigiem wątków.
    /// </summary>
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        #region ctor

        public BusinessLogicImplementation() : this(null)
        { }

        /// <summary>
        /// Konstruktor z wstrzykiwaniem zależności (Dependency Injection).
        /// Umożliwia przekazanie mocka warstwy danych w testach jednostkowych.
        /// </summary>
        internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
        {
            layerBellow = underneathLayer ?? UnderneathLayerAPI.GetDataLayer();
        }

        #endregion ctor

        #region BusinessLogicAbstractAPI

        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            layerBellow.Dispose();
            Disposed = true;
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            layerBellow.Start(numberOfBalls, (startingPosition, databall) =>
            {
                Ball logicBall = new Ball(databall);

                upperLayerHandler(
                    new Position(startingPosition.x, startingPosition.y),
                    logicBall);

                // Rejestracja obsługi zdarzenia - programowanie reaktywne.
                // Każde nowe położenie kuli wyzwala detekcję kolizji (asynchronicznie).
                databall.NewPositionNotification += (sender, newPosition) =>
                {
                    // Globalny lock zapobiega wyścigowi wątków przy równoczesnej detekcji kolizji
                    lock (_collisionLock)
                    {
                        CheckWallCollisions(databall);
                        CheckBallCollisions(databall);
                    }
                };
            });
        }

        #endregion BusinessLogicAbstractAPI

        #region private

        private bool Disposed = false;
        private readonly UnderneathLayerAPI layerBellow;

        /// <summary>
        /// Globalny lock sekcji krytycznej detekcji kolizji.
        /// Zapobiega równoczesnemu modyfikowaniu prędkości kul przez wiele wątków.
        /// </summary>
        private readonly object _collisionLock = new object();

        /// <summary>
        /// Sprawdza i obsługuje odbicia od ścian planszy.
        /// Odwraca składową prędkości przy kontakcie ze ścianą.
        /// </summary>
        private void CheckWallCollisions(Data.IBall ball)
        {
            double diameter = ball.Radius * 2;

            if (ball.Position.x < 0)
            {
                ball.Position.x = 0;
                if (ball.Velocity.x < 0)
                    ball.Velocity.x = -ball.Velocity.x;
            }
            else if (ball.Position.x + diameter > layerBellow.BoardWidth)
            {
                ball.Position.x = layerBellow.BoardWidth - diameter;
                if (ball.Velocity.x > 0)
                    ball.Velocity.x = -ball.Velocity.x;
            }

            if (ball.Position.y < 0)
            {
                ball.Position.y = 0;
                if (ball.Velocity.y < 0)
                    ball.Velocity.y = -ball.Velocity.y;
            }
            else if (ball.Position.y + diameter > layerBellow.BoardHeight)
            {
                ball.Position.y = layerBellow.BoardHeight - diameter;
                if (ball.Velocity.y > 0)
                    ball.Velocity.y = -ball.Velocity.y;
            }
        }

        /// <summary>
        /// Sprawdza kolizje między kulami i aktualizuje ich prędkości
        /// zgodnie z zasadami zachowania pędu i energii kinetycznej (zderzenie elastyczne).
        /// </summary>
        private void CheckBallCollisions(Data.IBall ball)
        {
            foreach (Data.IBall other in layerBellow.GetBalls())
            {
                if (ReferenceEquals(ball, other)) continue;

                double dx = other.Position.x - ball.Position.x;
                double dy = other.Position.y - ball.Position.y;
                double distanceSq = dx * dx + dy * dy;
                double minDist = ball.Radius + other.Radius;

                if (distanceSq > minDist * minDist) continue;

                double distance = Math.Sqrt(distanceSq);
                if (distance < 1e-9) continue; // Unikamy dzielenia przez zero

                // Sprawdź czy kule się zbliżają (dot product prędkości względnej i wektora normalnego)
                double dvx = other.Velocity.x - ball.Velocity.x;
                double dvy = other.Velocity.y - ball.Velocity.y;
                if (dx * dvx + dy * dvy >= 0) continue; // Kule się oddalają - nie kolizja

                // Wektory: normalny (n) i styczny (t)
                double nx = dx / distance;
                double ny = dy / distance;
                double tx = -ny;
                double ty = nx;

                // Rzuty prędkości na oś normalną i styczną
                double v1n = ball.Velocity.x * nx + ball.Velocity.y * ny;
                double v2n = other.Velocity.x * nx + other.Velocity.y * ny;
                double v1t = ball.Velocity.x * tx + ball.Velocity.y * ty;
                double v2t = other.Velocity.x * tx + other.Velocity.y * ty;

                double m1 = ball.Mass;
                double m2 = other.Mass;
                double totalMass = m1 + m2;

                // Zderzenie elastyczne - zachowanie pędu na osi normalnej
                double v1nAfter = (v1n * (m1 - m2) + 2 * m2 * v2n) / totalMass;
                double v2nAfter = (v2n * (m2 - m1) + 2 * m1 * v1n) / totalMass;

                // Składanie prędkości ze składowej normalnej i stycznej
                ball.Velocity.x = tx * v1t + nx * v1nAfter;
                ball.Velocity.y = ty * v1t + ny * v1nAfter;
                other.Velocity.x = tx * v2t + nx * v2nAfter;
                other.Velocity.y = ty * v2t + ny * v2nAfter;

                // Separacja kul (zapobieganie nakładaniu)
                double overlap = minDist - distance;
                double correctionRatio = overlap / 2.0 / distance;
                ball.Position.x -= dx * correctionRatio;
                ball.Position.y -= dy * correctionRatio;
                other.Position.x += dx * correctionRatio;
                other.Position.y += dy * correctionRatio;
            }
        }

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}