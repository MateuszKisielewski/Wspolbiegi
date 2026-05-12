//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TP.ConcurrentProgramming.BusinessLogic;

namespace TP.ConcurrentProgramming.BusinessLogicTest
{
    internal class VectorFixture : TP.ConcurrentProgramming.Data.IVector
    {
        public double x { get; set; }
        public double y { get; set; }
    }

    internal class DataBallFixture : TP.ConcurrentProgramming.Data.IBall
    {
        public TP.ConcurrentProgramming.Data.IVector Velocity { get; set; } = new VectorFixture();
        public TP.ConcurrentProgramming.Data.IVector Position { get; } = new VectorFixture();
        public double Mass { get; } = 1.0;
        public double Radius { get; } = 1.0;

        public event EventHandler<TP.ConcurrentProgramming.Data.IVector>? NewPositionNotification;

        public void Move()
        {
            NewPositionNotification?.Invoke(this, Position);
        }
    }

    [TestClass]
    public class BusinessBallUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            DataBallFixture dataBall = new();
            BusinessBall logicBall = new(dataBall);
            Assert.IsNotNull(logicBall);
        }
    }
}