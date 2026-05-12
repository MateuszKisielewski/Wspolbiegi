using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TP.ConcurrentProgramming.BusinessLogic;
using DataIBall = TP.ConcurrentProgramming.Data.IBall;
using DataIVector = TP.ConcurrentProgramming.Data.IVector;
using DataAbstractAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogicTest
{
    internal class VectorStub : DataIVector
    {
        public double x { get; set; } = 0;
        public double y { get; set; } = 0;
    }

    internal class BallStub : DataIBall
    {
        public DataIVector Velocity { get; set; } = new VectorStub();
        public DataIVector Position { get; } = new VectorStub();
        public double Mass { get; } = 1.0;
        public double Radius { get; } = 5.0;
        public event EventHandler<DataIVector>? NewPositionNotification;
        public void Move() { }
    }

    internal class DataLayerStub : DataAbstractAPI
    {
        public override int BoardWidth => 400;
        public override int BoardHeight => 400;
        public override void Start(int numberOfBalls, Action<DataIVector, DataIBall> upperLayerHandler)
        {
            for (int i = 0; i < numberOfBalls; i++)
                upperLayerHandler(new VectorStub(), new BallStub());
        }

        public override IEnumerable<DataIBall> GetBalls() => new List<DataIBall>();
        public override void Dispose() { }
    }

    [TestClass]
    public class BusinessBallUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            DataLayerStub stub = new();
            using BusinessLogicAbstractAPI logikaAPI = new BusinessLogicImplementation(stub);
            IBall? pobranaBall = null;
            logikaAPI.Start(1, (pozycja, kulka) => { pobranaBall = kulka; });
            Assert.IsNotNull(pobranaBall);
        }
    }
}