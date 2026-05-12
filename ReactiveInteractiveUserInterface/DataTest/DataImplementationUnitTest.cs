using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class DataImplementationUnitTest
    {
        [TestMethod]
        public void StartTestMethod()
        {
            using DataImplementation dataAPI = new DataImplementation();
            int ballCount = 0;
            dataAPI.Start(5, (pos, ball) => { Assert.IsNotNull(ball); ballCount++; });
            Assert.AreEqual<int>(5, ballCount);
            Assert.AreEqual<int>(5, dataAPI.GetBalls().Count());
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            DataImplementation dataAPI = new DataImplementation();
            dataAPI.Start(3, (pos, ball) => { Assert.IsNotNull(ball); });
            Assert.AreEqual<int>(3, dataAPI.GetBalls().Count());
            dataAPI.Dispose();
            Assert.AreEqual<int>(0, dataAPI.GetBalls().Count());
        }
    }
}