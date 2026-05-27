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

namespace TP.ConcurrentProgramming.Data
{
    public abstract class DataAbstractAPI : IDisposable
    {
        #region Layer Factory

        public static DataAbstractAPI GetDataLayer()
        {
            return modelInstance.Value;
        }

        #endregion Layer Factory

        #region public API

        public abstract int BoardWidth { get; }
        public abstract int BoardHeight { get; }

        /// <summary>
        /// Uruchamia symulację numberOfBalls kul.
        /// upperLayerHandler wywoływany dla każdej nowo utworzonej kuli.
        /// </summary>
        public abstract void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler);

        /// <summary>
        /// Zwraca kolekcję wszystkich aktywnych kul.
        /// Używane przez warstwę logiki do detekcji kolizji.
        /// </summary>
        public abstract IEnumerable<IBall> GetBalls();

        #endregion public API

        #region IDisposable

        public abstract void Dispose();

        #endregion IDisposable

        #region private

        private static Lazy<DataAbstractAPI> modelInstance =
            new Lazy<DataAbstractAPI>(() => new DataImplementation());

        #endregion private
    }

    public interface IVector
    {
        double x { get; set; }

        double y { get; set; }
    }

    public interface IBall
    {
        event EventHandler<IVector> NewPositionNotification;

        IVector Velocity { get; set; }
        IVector Position { get; }
        double Mass { get; }
        double Radius { get; }

        void Move();

        void SetVelocity(double vx, double vy);
        void AdjustPosition(double dX, double dY);
    }
}