//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2023, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TP.ConcurrentProgramming.BusinessLogic;
using LogicIBall = TP.ConcurrentProgramming.BusinessLogic.IBall;

namespace TP.ConcurrentProgramming.Presentation.Model
{
    /// <summary>
    /// Kula w warstwie modelu prezentacji.
    /// Przelicza współrzędne logiczne (400x400) na współrzędne ekranowe (CanvasWidth x CanvasHeight).
    /// Implementuje INotifyPropertyChanged - zmiana pozycji wyzwala automatyczne odświeżenie widoku (reaktywność WPF).
    /// </summary>
    internal class ModelBall : IBall
    {
        #region ctor

        public ModelBall(double top, double left, double diameter, LogicIBall underneathBall)
        {
            TopBackingField = top;
            LeftBackingField = left;
            _baseDiameter = diameter;

            // Subskrypcja zdarzenia z warstwy logiki - programowanie reaktywne
            underneathBall.NewPositionNotification += NewPositionNotification;
        }

        #endregion ctor

        #region IBall

        public double Top
        {
            get => TopBackingField;
            private set
            {
                if (TopBackingField == value) return;
                TopBackingField = value;
                RaisePropertyChanged();
            }
        }

        public double Left
        {
            get => LeftBackingField;
            private set
            {
                if (LeftBackingField == value) return;
                LeftBackingField = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Średnica skalowana do aktualnego rozmiaru canvas.
        /// Wartości współrzędnych są skalowane do aktualnego rozmiaru ekranu.
        /// </summary>
        public double Diameter
        {
            get
            {
                double scale = ModelAbstractApi.CanvasWidth / LogicalBoardSize;
                return _baseDiameter * scale;
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion INotifyPropertyChanged

        #endregion IBall

        #region private

        private double TopBackingField;
        private double LeftBackingField;
        private readonly double _baseDiameter;

        /// <summary>
        /// Logiczny rozmiar planszy - taki sam jak BoardWidth/BoardHeight w DataImplementation.
        /// </summary>
        private const double LogicalBoardSize = 400.0;

        /// <summary>
        /// Obsługa zdarzenia zmiany pozycji z warstwy logiki.
        /// Skaluje współrzędne logiczne do rozmiaru ekranu i aktualizuje właściwości.
        /// Wywoływana z wątku kuli - WPF data binding obsługuje to bezpiecznie przez Dispatcher.
        /// </summary>
        private void NewPositionNotification(object? sender, IPosition e)
        {
            double scaleX = ModelAbstractApi.CanvasWidth / LogicalBoardSize;
            double scaleY = ModelAbstractApi.CanvasHeight / LogicalBoardSize;
            double diameter = Diameter;

            // Clamp zapobiega wychodzeniu kuli poza obszar canvas
            Left = Math.Clamp(e.x * scaleX, 0, ModelAbstractApi.CanvasWidth - diameter);
            Top = Math.Clamp(e.y * scaleY, 0, ModelAbstractApi.CanvasHeight - diameter);

            // Powiadom o zmianie Diameter gdy zmienił się rozmiar canvas
            RaisePropertyChanged(nameof(Diameter));
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion private

        #region testing instrumentation

        [Conditional("DEBUG")]
        internal void SetLeft(double x) { Left = x; }

        [Conditional("DEBUG")]
        internal void SetTop(double x) { Top = x; }

        #endregion testing instrumentation
    }
}