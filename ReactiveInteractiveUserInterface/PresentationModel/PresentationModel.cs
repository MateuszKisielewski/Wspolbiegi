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
    /// <summary>
    /// Implementacja modelu prezentacji.
    /// Używa Reactive Extensions (Rx) - programowanie reaktywne - do powiadamiania ViewModelu
    /// o nowych kulach przez IObservable&lt;IBall&gt;.
    /// Wstrzykiwanie zależności (DI) w konstruktorze umożliwia podmianę warstwy logiki w testach.
    /// </summary>
    internal class ModelImplementation : ModelAbstractApi
    {
        #region ctor

        internal ModelImplementation() : this(null)
        { }

        /// <summary>
        /// Konstruktor z Dependency Injection - przyjmuje warstwę logiki lub tworzy domyślną.
        /// </summary>
        internal ModelImplementation(UnderneathLayerAPI? underneathLayer)
        {
            layerBellow = underneathLayer ?? UnderneathLayerAPI.GetBusinessLogicLayer();

            // Observable.FromEventPattern konwertuje zdarzenie BallChanged na strumień Rx
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

        /// <summary>
        /// Subskrybuje obserwatora na strumień nowych kul.
        /// Reaktywne: obserwator reaguje na każdą nową kulę bez aktywnego odpytywania (polling).
        /// </summary>
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

        /// <summary>
        /// Handler wywoływany przez warstwę logiki dla każdej nowej kuli.
        /// Tworzy ModelBall ze skalowanymi współrzędnymi i emituje zdarzenie BallChanged.
        /// </summary>
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

    /// <summary>
    /// Argumenty zdarzenia zmiany kuli.
    /// </summary>
    public class BallChangedEventArgs : EventArgs
    {
        public IBall Ball { get; init; } = null!;
    }
}