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

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall, IDisposable
    {
        private Timer _timer;
        private readonly object _positionLock = new object();
        private Stopwatch _stopwatch = new Stopwatch();

        private readonly string _ballId;
        private static int _ballCounter = 0;

        private const double BaseSpeed = 150.0;

        internal Ball(IVector initialPosition, double mass, double radius)
        {
            _ballId = $"B{Interlocked.Increment(ref _ballCounter):D3}";

            Position = initialPosition;
            Mass = mass;
            Radius = radius;

            Random rand = new Random();

            double angle = rand.NextDouble() * 2.0 * Math.PI;
            double speed = BaseSpeed * (0.5 + rand.NextDouble());

            Velocity = new Vector(
                speed * Math.Cos(angle),
                speed * Math.Sin(angle)
            );
        }

        public event EventHandler<IVector> NewPositionNotification;

        public IVector Velocity { get; set; }
        public IVector Position { get; private set; }
        public double Mass { get; }
        public double Radius { get; }

        public void Move()
        {
            MoveTick(null);
        }

        public void SetVelocity(double vx, double vy)
        {
            lock (_positionLock)
            {
                Velocity.x = vx;
                Velocity.y = vy;
            }
        }

        public void AdjustPosition(double dx, double dy)
        {
            lock (_positionLock)
            {
                Position.x += dx;
                Position.y += dy;
            }
        }

        internal void StartMoving()
        {
            _stopwatch.Start();
            _timer = new Timer(MoveTick, null, 0, 16);
        }

        public void StopMoving()
        {
            _timer?.Change(Timeout.Infinite, 0);
            _stopwatch.Stop();
        }

        private void MoveTick(object state)
        {
            lock (_positionLock)
            {
                double deltaSeconds = _stopwatch.Elapsed.TotalSeconds;
                _stopwatch.Restart();

                if (deltaSeconds > 0.1) deltaSeconds = 0.1;

                double newX = Position.x + Velocity.x * deltaSeconds;
                double newY = Position.y + Velocity.y * deltaSeconds;
                Position.x = newX;
                Position.y = newY;

                DiagnosticLogger.Instance.Log(_ballId, newX, newY, Velocity.x, Velocity.y, deltaSeconds * 1000.0);
            }

            NewPositionNotification?.Invoke(this, Position);
        }

        internal void ExecuteUnderLock(Action action)
        {
            lock (_positionLock)
            {
                action();
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }
}