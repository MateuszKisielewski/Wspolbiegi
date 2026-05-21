//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.ComponentModel;

namespace TP.ConcurrentProgramming.Presentation.Model
{
    /// <summary>
    /// Interfejs kuli widoczny dla ViewModelu - wspiera INotifyPropertyChanged
    /// co umożliwia reaktywne wiązanie danych w WPF (data binding).
    /// </summary>
    public interface IBall : INotifyPropertyChanged
    {
        double Top { get; }
        double Left { get; }
        double Diameter { get; }
    }

    /// <summary>
    /// Abstrakcyjne API warstwy modelu prezentacji.
    /// Implementuje IObservable&lt;IBall&gt; - programowanie reaktywne (Rx).
    /// Wstrzykiwanie zależności (DI) przez konstruktor w ModelImplementation.
    /// </summary>
    public abstract class ModelAbstractApi : IObservable<IBall>, IDisposable
    {
        /// <summary>
        /// Szerokość obszaru rysowania - ustawiana przez widok przed startem symulacji.
        /// </summary>
        public static double CanvasWidth { get; set; } = 400;

        /// <summary>
        /// Wysokość obszaru rysowania - ustawiana przez widok przed startem symulacji.
        /// </summary>
        public static double CanvasHeight { get; set; } = 400;

        public static ModelAbstractApi CreateModel()
        {
            return modelInstance.Value;
        }

        public abstract void Start(int numberOfBalls);

        #region IObservable

        public abstract IDisposable Subscribe(IObserver<IBall> observer);

        #endregion IObservable

        #region IDisposable

        public abstract void Dispose();

        #endregion IDisposable

        #region private

        private static Lazy<ModelAbstractApi> modelInstance =
            new Lazy<ModelAbstractApi>(() => new ModelImplementation());

        #endregion private
    }
}