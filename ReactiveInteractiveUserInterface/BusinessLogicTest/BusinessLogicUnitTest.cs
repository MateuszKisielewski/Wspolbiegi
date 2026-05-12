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
using System.Collections.Generic;
using System.Linq;
using TP.ConcurrentProgramming.BusinessLogic;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogicTest
{
    internal class DataLayerStub : DataAbstractAPI
    {
        private List<TP.ConcurrentProgramming.Data.IBall> _listaKulek = new();

        public override int BoardWidth => 100;
        public override int BoardHeight => 100;

        public override void Start(int numberOfBalls, Action<TP.ConcurrentProgramming.Data.IVector, TP.ConcurrentProgramming.Data.IBall> upperLayerHandler)
        {
            for (int i = 0; i < numberOfBalls; i++)
            {
                Ball nowaKulka = new(new Vector(50, 50), 10, 5);
                _listaKulek.Add(nowaKulka);
                upperLayerHandler(nowaKulka.Position, nowaKulka);
            }
        }

        public override IEnumerable<TP.ConcurrentProgramming.Data.IBall> GetBalls() => _listaKulek;

        public override void Dispose()
        {
            foreach (TP.ConcurrentProgramming.Data.IBall kulka in _listaKulek)
            {
                if (kulka is IDisposable disposableBall)
                {
                    disposableBall.Dispose();
                }
            }
            _listaKulek.Clear();
        }
    }

    [TestClass]
    public class BusinessLogicUnitTest
    {
        [TestMethod]
        public void StartTestMethod()
        {
            DataLayerStub testowyStub = new();
            int licznikWywolanHandlera = 0;
            using BusinessLogicAbstractAPI logikaAPI = BusinessLogicAbstractAPI.GetBusinessLogicLayer(testowyStub);
            logikaAPI.Start(3, (pozycja, kulkaLogiki) => { Assert.IsNotNull(kulkaLogiki); licznikWywolanHandlera++; });
            Assert.AreEqual<int>(3, licznikWywolanHandlera);
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            DataLayerStub testowyStub = new();
            BusinessLogicAbstractAPI logikaAPI = BusinessLogicAbstractAPI.GetBusinessLogicLayer(testowyStub);
            logikaAPI.Start(2, (pozycja, kulkaLogiki) => { Assert.IsNotNull(kulkaLogiki); });

            List<TP.ConcurrentProgramming.Data.IBall> pobraneKulki = testowyStub.GetBalls().ToList();
            Assert.AreEqual<int>(2, pobraneKulki.Count);

            logikaAPI.Dispose();

            pobraneKulki = testowyStub.GetBalls().ToList();
            Assert.AreEqual<int>(0, pobraneKulki.Count);
        }
    }
}