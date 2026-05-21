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

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor

        public DataImplementation()
        {
        }

        #endregion ctor

        #region DataAbstractAPI

        public override int BoardWidth => 400;
        public override int BoardHeight => 400;

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            Random random = new Random();
            for (int i = 0; i < numberOfBalls; i++)
            {
                double radius = random.Next(10, 20);
                double mass = radius;

                // Pozycja startowa z marginesem równym promieniowi kuli
                double startX = random.NextDouble() * (BoardWidth - 2 * radius) + radius;
                double startY = random.NextDouble() * (BoardHeight - 2 * radius) + radius;

                Vector startingPosition = new Vector(startX, startY);
                Ball newBall = new Ball(startingPosition, mass, radius);
                newBall.StartMoving();

                lock (_ballsLock)
                {
                    BallsList.Add(newBall);
                }

                upperLayerHandler(startingPosition, newBall);
            }
        }

        public override IEnumerable<IBall> GetBalls()
        {
            lock (_ballsLock)
            {
                // Zwracamy kopię listy, żeby uniknąć blokowania podczas iteracji
                return new List<IBall>(BallsList);
            }
        }

        #endregion DataAbstractAPI

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    lock (_ballsLock)
                    {
                        foreach (var ball in BallsList)
                            (ball as IDisposable)?.Dispose();
                        BallsList.Clear();
                    }

                    // Zamknij logger diagnostyczny
                    DiagnosticLogger.Instance.Dispose();
                }
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        private bool Disposed = false;
        private readonly List<IBall> BallsList = new List<IBall>();
        private readonly object _ballsLock = new object();

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            returnBallsList(BallsList);
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            returnNumberOfBalls(BallsList.Count);
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}