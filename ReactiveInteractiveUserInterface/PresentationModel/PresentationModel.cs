//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//_____________________________________________________________________________________________________________________________________

using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using UnderneathLayerAPI = TP.ConcurrentProgramming.BusinessLogic.BusinessLogicAbstractAPI;

namespace TP.ConcurrentProgramming.Presentation.Model
{
    internal class ModelImplementation : ModelAbstractApi
    {
        #region ctor

        internal ModelImplementation() : this(null)
        { }

        internal ModelImplementation(UnderneathLayerAPI? underneathLayer)
        {
            layerBellow = underneathLayer ?? UnderneathLayerAPI.GetBusinessLogicLayer();

            eventObservable = Observable.FromEventPattern<BallChangedEventArgs>(
                this, nameof(BallChanged));
        }

        #endregion ctor

        #region ModelAbstractApi

        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(ModelImplementation));
            layerBellow.Dispose();
            Disposed = true;
        }

        public override IDisposable Subscribe(IObserver<IBall> observer)
        {
            return eventObservable.Subscribe(
                x => observer.OnNext(x.EventArgs.Ball),
                ex => observer.OnError(ex),
                () => observer.OnCompleted());
        }

        public override void Start(int numberOfBalls)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(ModelImplementation));
            layerBellow.Start(numberOfBalls, StartHandler);
        }

        #endregion ModelAbstractApi

        #region API

        public event EventHandler<BallChangedEventArgs>? BallChanged;

        #endregion API

        #region private

        private bool Disposed = false;
        private readonly IObservable<EventPattern<BallChangedEventArgs>> eventObservable;
        private readonly UnderneathLayerAPI layerBellow;

        private const double LogicalBoardSize = 400.0;

        private void StartHandler(BusinessLogic.IPosition position, BusinessLogic.IBall ball)
        {
            double scaleX = ModelAbstractApi.CanvasWidth / LogicalBoardSize;
            double scaleY = ModelAbstractApi.CanvasHeight / LogicalBoardSize;

            double scaledLeft = position.x * scaleX;
            double scaledTop = position.y * scaleY;

            ModelBall newBall = new ModelBall(scaledTop, scaledLeft, ball.Diameter, ball);

            BallChanged?.Invoke(this, new BallChangedEventArgs { Ball = newBall });
        }

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        [Conditional("DEBUG")]
        internal void CheckUnderneathLayerAPI(Action<UnderneathLayerAPI> returnLayer)
        {
            returnLayer(layerBellow);
        }

        [Conditional("DEBUG")]
        internal void CheckBallChangedEvent(Action<bool> returnBallChangedIsNull)
        {
            returnBallChangedIsNull(BallChanged == null);
        }

        #endregion TestingInfrastructure
    }

    public class BallChangedEventArgs : EventArgs
    {
        public IBall Ball { get; init; } = null!;
    }
}