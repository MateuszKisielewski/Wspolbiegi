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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
    /// <summary>
    /// Reprezentuje kulę w warstwie danych.
    /// Ruch oparty na czasie rzeczywistym (Stopwatch + delta time) - programowanie czasu rzeczywistego.
    /// Każda kula porusza się we własnym wątku (Task).
    /// </summary>
    internal class Ball : IBall, IDisposable
    {
        #region private fields

        private volatile bool _isRunning;
        private Task? _moveTask;

        // Synchronizacja dostępu do pozycji i prędkości - sekcja krytyczna
        private readonly object _stateLock = new object();

        // Stopwatch do pomiaru rzeczywistego czasu - programowanie czasu rzeczywistego
        private readonly Stopwatch _stopwatch = new Stopwatch();

        // Identyfikator kuli do logowania
        private readonly string _ballId;

        // Globalny licznik kul
        private static int _ballCounter = 0;

        // Docelowy interwał kroku symulacji [ms]
        private const double TargetStepMs = 16.0; // ~60 fps

        // Prędkość bazowa [piksele/sekundę] - uwzględniana z delta time
        private const double BaseSpeed = 150.0;

        #endregion private fields

        #region ctor

        internal Ball(IVector initialPosition, double mass, double radius)
        {
            _ballId = $"B{Interlocked.Increment(ref _ballCounter):D3}";

            Position = initialPosition;
            Mass = mass;
            Radius = radius;

            Random rand = new Random();

            // Prędkość losowa - wartości w jednostkach logicznych/sekundę
            double angle = rand.NextDouble() * 2.0 * Math.PI;
            double speed = BaseSpeed * (0.5 + rand.NextDouble()); // 75..150 px/s

            Velocity = new Vector(
                speed * Math.Cos(angle),
                speed * Math.Sin(angle)
            );
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity { get; set; }
        public IVector Position { get; private set; }
        public double Mass { get; }
        public double Radius { get; }

        /// <summary>
        /// Przesuwa kulę o jeden krok. Wywoływana zewnętrznie tylko w testach.
        /// W produkcji ruch odbywa się przez StartMoving().
        /// </summary>
        public void Move()
        {
            MoveByDelta(TargetStepMs / 1000.0);
        }

        public void SetVelocity(double vx, double vy)
        {
            lock (_stateLock)
            {
                Velocity.x = vx;
                Velocity.y = vy;
            }
        }

        public void AdjustPosition(double dx, double dy)
        {
            lock (_stateLock)
            {
                Position.x += dx;
                Position.y += dy;
            }
        }

        #endregion IBall

        #region internal

        internal void StartMoving()
        {
            _isRunning = true;
            _stopwatch.Start();
            _moveTask = Task.Run(MoveLoopAsync);
        }

        #endregion internal

        #region private

        /// <summary>
        /// Pętla ruchu - programowanie czasu rzeczywistego.
        /// Delta time mierzony Stopwatchem zapewnia, że prędkość kul jest
        /// niezależna od obciążenia systemu.
        /// </summary>
        private async Task MoveLoopAsync()
        {
            long previousTicks = _stopwatch.ElapsedTicks;

            while (_isRunning)
            {
                long currentTicks = _stopwatch.ElapsedTicks;
                double deltaSeconds = (currentTicks - previousTicks) / (double)Stopwatch.Frequency;
                previousTicks = currentTicks;

                // Ograniczenie delta aby uniknąć skoku po pauzie/debugowaniu
                if (deltaSeconds > 0.1) deltaSeconds = 0.1;

                MoveByDelta(deltaSeconds);

                // Oblicz czas do następnego kroku - adaptacyjne opóźnienie
                long elapsed = _stopwatch.ElapsedTicks - currentTicks;
                double elapsedMs = elapsed * 1000.0 / Stopwatch.Frequency;
                int delay = Math.Max(1, (int)(TargetStepMs - elapsedMs));
                await Task.Delay(delay);
            }
        }

        /// <summary>
        /// Przesuwa kulę uwzględniając rzeczywisty czas deltaSeconds.
        /// Sekcja krytyczna chroni Position i Velocity przed równoczesnym dostępem
        /// z wątku logiki (detekcja kolizji).
        /// </summary>
        private void MoveByDelta(double deltaSeconds)
        {
            double newX, newY, vx, vy;

            lock (_stateLock)
            {
                newX = Position.x + Velocity.x * deltaSeconds;
                newY = Position.y + Velocity.y * deltaSeconds;
                Position.x = newX;
                Position.y = newY;
                vx = Velocity.x;
                vy = Velocity.y;
            }

            // Logowanie diagnostyczne - timestamp w milisekundach
            double timestamp = _stopwatch.Elapsed.TotalMilliseconds;
            DiagnosticLogger.Instance.Log(_ballId, newX, newY, vx, vy, timestamp);

            RaiseNewPositionChangeNotification();
        }

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        #endregion private

        #region IDisposable

        public void Dispose()
        {
            _isRunning = false;
            _stopwatch.Stop();
            if (_moveTask != null)
            {
                try { _moveTask.Wait(500); } catch { }
            }
        }

        #endregion IDisposable

        #region internal - lock accessor for BusinessLogic collision detection

        /// <summary>
        /// Wykonuje akcję z blokadą stanu kuli.
        /// Używane przez warstwę logiki do bezpiecznej detekcji kolizji.
        /// </summary>
        internal void ExecuteUnderLock(Action action)
        {
            lock (_stateLock)
            {
                action();
            }
        }

        #endregion
    }
}