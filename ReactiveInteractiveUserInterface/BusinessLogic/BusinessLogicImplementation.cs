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
using System.Threading.Tasks;
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
            layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
        }

        #endregion ctor

        #region BusinessLogicAbstractAPI

        public override void Dispose()
        {
            if (Disposed)
                return;

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
                var logicBall = new Ball(databall);
                upperLayerHandler(new Position(startingPosition.x, startingPosition.y), logicBall);

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
        private readonly object _collisionLock = new object();

        private void CheckWallCollisions(Data.IBall ball)
        {
            if (ball.Position.x <= 0)
            {
                ball.Position.x = 0;
                if (ball.Velocity.x < 0) ball.Velocity.x = -ball.Velocity.x;
            }
            else if (ball.Position.x + ball.Radius * 2 >= layerBellow.BoardWidth)
            {
                ball.Position.x = layerBellow.BoardWidth - ball.Radius * 2;
                if (ball.Velocity.x > 0) ball.Velocity.x = -ball.Velocity.x;
            }

            if (ball.Position.y <= 0)
            {
                ball.Position.y = 0;
                if (ball.Velocity.y < 0) ball.Velocity.y = -ball.Velocity.y;
            }
            else if (ball.Position.y + ball.Radius * 2 >= layerBellow.BoardHeight)
            {
                ball.Position.y = layerBellow.BoardHeight - ball.Radius * 2;
                if (ball.Velocity.y > 0) ball.Velocity.y = -ball.Velocity.y;
            }
        }

        private void CheckBallCollisions(Data.IBall ball)
        {
            foreach (var otherBall in layerBellow.GetBalls())
            {
                if (ball == otherBall) continue;

                double dx = otherBall.Position.x - ball.Position.x;
                double dy = otherBall.Position.y - ball.Position.y;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance <= ball.Radius + otherBall.Radius)
                {
                    double dvx = otherBall.Velocity.x - ball.Velocity.x;
                    double dvy = otherBall.Velocity.y - ball.Velocity.y;

                    if (dx * dvx + dy * dvy > 0)
                        continue;

                    double nx = dx / distance;
                    double ny = dy / distance;

                    double tx = -ny;
                    double ty = nx;

                    double dpTan1 = ball.Velocity.x * tx + ball.Velocity.y * ty;
                    double dpTan2 = otherBall.Velocity.x * tx + otherBall.Velocity.y * ty;

                    double dpNorm1 = ball.Velocity.x * nx + ball.Velocity.y * ny;
                    double dpNorm2 = otherBall.Velocity.x * nx + otherBall.Velocity.y * ny;

                    double m1 = (dpNorm1 * (ball.Mass - otherBall.Mass) + 2.0 * otherBall.Mass * dpNorm2) / (ball.Mass + otherBall.Mass);
                    double m2 = (dpNorm2 * (otherBall.Mass - ball.Mass) + 2.0 * ball.Mass * dpNorm1) / (ball.Mass + otherBall.Mass);

                    ball.Velocity.x = tx * dpTan1 + nx * m1;
                    ball.Velocity.y = ty * dpTan1 + ny * m1;
                    otherBall.Velocity.x = tx * dpTan2 + nx * m2;
                    otherBall.Velocity.y = ty * dpTan2 + ny * m2;
                }
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