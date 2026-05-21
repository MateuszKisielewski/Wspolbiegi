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

namespace TP.ConcurrentProgramming.BusinessLogic
{
    /// <summary>
    /// Abstrakcyjne API warstwy logiki biznesowej.
    /// Wyraźnie wydzielona abstrakcja umożliwia niezależne testowanie jednostkowe i wstrzykiwanie zależności (DI).
    /// </summary>
    public abstract class BusinessLogicAbstractAPI : IDisposable
    {
        #region Layer Factory

        public static BusinessLogicAbstractAPI GetBusinessLogicLayer()
        {
            return modelInstance.Value;
        }

        #endregion Layer Factory

        #region Layer API

        /// <summary>
        /// Wymiary logiczne planszy - niezależne od rozmiaru ekranu.
        /// Skalowanie do ekranu odbywa się w warstwach wyżej.
        /// </summary>
        public static readonly Dimensions GetDimensions = new(10.0, 10.0, 10.0);

        /// <summary>
        /// Uruchamia symulację z podaną liczbą kul.
        /// upperLayerHandler wywoływany przy każdym nowo dodanym obiekcie kuli.
        /// </summary>
        public abstract void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler);

        #region IDisposable

        public abstract void Dispose();

        #endregion IDisposable

        #endregion Layer API

        #region private

        private static Lazy<BusinessLogicAbstractAPI> modelInstance =
            new Lazy<BusinessLogicAbstractAPI>(() => new BusinessLogicImplementation());

        #endregion private
    }

    /// <summary>
    /// Niemutowalne wymiary planszy.
    /// </summary>
    public record Dimensions(double BallDimension, double TableHeight, double TableWidth);

    /// <summary>
    /// Pozycja kuli w przestrzeni logicznej.
    /// </summary>
    public interface IPosition
    {
        double x { get; init; }
        double y { get; init; }
    }

    /// <summary>
    /// Interfejs kuli widoczny dla warstwy prezentacji.
    /// Zdarzenie NewPositionNotification jest reaktywnym mechanizmem powiadamiania.
    /// </summary>
    public interface IBall
    {
        event EventHandler<IPosition> NewPositionNotification;
        double Diameter { get; }
    }
}