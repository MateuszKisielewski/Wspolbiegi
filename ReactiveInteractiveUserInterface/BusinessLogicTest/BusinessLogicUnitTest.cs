using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TP.ConcurrentProgramming.BusinessLogic;
using DataIBall = TP.ConcurrentProgramming.Data.IBall;
using DataIVector = TP.ConcurrentProgramming.Data.IVector;
using DataAbstractAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogicTest
{
    [TestClass]
    public class BusinessLogicUnitTest
    {
        [TestMethod]
        public void StartTestMethod()
        {
            int licznikWywolanHandlera = 0;
            using BusinessLogicAbstractAPI logikaAPI = new BusinessLogicImplementation(new DataLayerStub());
            logikaAPI.Start(3, (pozycja, kulkaLogiki) => { Assert.IsNotNull(kulkaLogiki); licznikWywolanHandlera++; });
            Assert.AreEqual<int>(3, licznikWywolanHandlera);
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            BusinessLogicAbstractAPI logikaAPI = new BusinessLogicImplementation(new DataLayerStub());
            logikaAPI.Start(2, (pozycja, kulkaLogiki) => { Assert.IsNotNull(kulkaLogiki); });
            logikaAPI.Dispose();
        }
    }
}