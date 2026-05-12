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
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class DataImplementationUnitTest
    {
        [TestMethod]
        public void StartTestMethod()
        {
            using DataAbstractAPI dataAPI = DataAbstractAPI.GetDataLayer();
            int ballCount = 0;
            dataAPI.Start(5, (pos, ball) => { Assert.IsNotNull(ball); ballCount++; });
            Assert.AreEqual<int>(5, ballCount);
            Assert.AreEqual<int>(5, dataAPI.GetBalls().Count);
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            DataAbstractAPI dataAPI = DataAbstractAPI.GetDataLayer();
            dataAPI.Start(3, (pos, ball) => { Assert.IsNotNull(ball); });
            Assert.AreEqual<int>(3, dataAPI.GetBalls().Count);
            dataAPI.Dispose();
            Assert.AreEqual<int>(0, dataAPI.GetBalls().Count);
        }
    }
}