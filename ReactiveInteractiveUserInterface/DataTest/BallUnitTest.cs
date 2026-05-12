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
using System.Threading.Tasks;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            Vector testingVector = new(10.0, 10.0);
            Ball newInstance = new(testingVector, 5.0, 10.0);
            Assert.AreEqual<double>(10.0, newInstance.Position.x);
            Assert.AreEqual<double>(10.0, newInstance.Position.y);
            Assert.AreEqual<double>(5.0, newInstance.Mass);
            Assert.AreEqual<double>(10.0, newInstance.Radius);
        }

        [TestMethod]
        public async Task AsynchronousMovementTestMethod()
        {
            Vector initialPosition = new(10.0, 10.0);
            using Ball newInstance = new(initialPosition, 1.0, 10.0);
            bool positionChanged = false;
            newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); positionChanged = true; };
            await Task.Delay(100);
            Assert.IsTrue(positionChanged);
            Assert.AreNotEqual<double>(10.0, newInstance.Position.x);
            Assert.AreNotEqual<double>(10.0, newInstance.Position.y);
        }
    }
}