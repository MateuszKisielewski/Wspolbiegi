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
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        #region ctor

        public BusinessLogicImplementation() : this(null)
        { }

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

                databall.NewPositionNotification += (sender, newPosition) =>
                {
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
            double px = ball.Position.x;
            double py = ball.Position.y;
            double vx = ball.Velocity.x;
            double vy = ball.Velocity.y;

            if (px < 0)
            {
                ball.AdjustPosition(-px, 0);
                if (vx < 0) ball.SetVelocity(-vx, vy);
            }
            else if (px + diameter > layerBellow.BoardWidth)
            {
                ball.AdjustPosition((layerBellow.BoardWidth - diameter) - px, 0);
                if (vx > 0) ball.SetVelocity(-vx, vy);
            }

            vx = ball.Velocity.x;

            if (py < 0)
            {
                ball.AdjustPosition(0, -py);
                if (vy < 0) ball.SetVelocity(vx, -vy);
            }
            else if (py + diameter > layerBellow.BoardHeight)
            {
                ball.AdjustPosition(0, (layerBellow.BoardHeight - diameter) - py);
                if (vy > 0) ball.SetVelocity(vx, -vy);
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
                if (distance < 1e-9) continue;

                double dvx = other.Velocity.x - ball.Velocity.x;
                double dvy = other.Velocity.y - ball.Velocity.y;
                if (dx * dvx + dy * dvy >= 0) continue;

                double nx = dx / distance;
                double ny = dy / distance;
                double tx = -ny;
                double ty = nx;

                double v1n = ball.Velocity.x * nx + ball.Velocity.y * ny;
                double v2n = other.Velocity.x * nx + other.Velocity.y * ny;
                double v1t = ball.Velocity.x * tx + ball.Velocity.y * ty;
                double v2t = other.Velocity.x * tx + other.Velocity.y * ty;

                double m1 = ball.Mass;
                double m2 = other.Mass;
                double totalMass = m1 + m2;

                double v1nAfter = (v1n * (m1 - m2) + 2 * m2 * v2n) / totalMass;
                double v2nAfter = (v2n * (m2 - m1) + 2 * m1 * v1n) / totalMass;

                ball.SetVelocity(tx * v1t + nx * v1nAfter, ty * v1t + ny * v1nAfter);
                other.SetVelocity(tx * v2t + nx * v2nAfter, ty * v2t + ny * v2nAfter);

                double overlap = minDist - distance;
                double correctionRatio = overlap / 2.0 / distance;
                
                double adjustX = dx * correctionRatio;
                double adjustY = dy * correctionRatio;
                
                ball.AdjustPosition(-adjustX, -adjustY);
                other.AdjustPosition(adjustX, adjustY);
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