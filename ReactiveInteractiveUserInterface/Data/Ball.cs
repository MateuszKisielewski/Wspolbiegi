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

namespace TP.ConcurrentProgramming.Data
{
  internal class Ball : IBall
  {
    #region ctor

    internal Ball(IVector initialPosition, double mass, double radius)
    {
      Position = initialPosition;
      Mass = mass;
      Radius = radius;

      Random rand = new Random();
      double vx = (rand.NextDouble() * 4.0) + 1.0;
      if (rand.Next(2) == 0) vx = -vx;

      double vy = (rand.NextDouble() * 4.0) + 1.0;
      if (rand.Next(2) == 0) vy = -vy;

      Velocity = new Vector(vx, vy);
    }

    #endregion ctor

    #region IBall

    public event EventHandler<IVector>? NewPositionNotification;

    public IVector Velocity { get; set; }
    public IVector Position { get; private set; }
    public double Mass { get; }
    public double Radius { get; }

    public void Move()
    {
      Position.x += Velocity.x;
      Position.y += Velocity.y;
      RaiseNewPositionChangeNotification();
    }

    #endregion IBall

    #region private

    private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }

    #endregion private
  }
}