﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RMarket.ClassLib.EntityModels;
using RMarket.UnitTests.Infrastructure.Entities;
using RMarket.UnitTests.Infrastructure.Strategies;
using RMarket.ClassLib.Models;
using RMarket.ClassLib.Abstract;
using RMarket.ClassLib.Managers;
using RMarket.ClassLib.Abstract.IRepository;
using RMarket.UnitTests.Infrastructure.Repositories;
using System.Threading;

namespace RMarket.UnitTests.Tester
{
    [TestClass]
    public class TesterManagerTests
    {
        [TestMethod]
        public void RunTestStrategy()
        {
            //создаем инстанс
            InstanceModel instance = Instances.GetInstance1();

            //получаем стратегию 
            StrategyMock1 strategy = new StrategyMock1();

            //устанавливаем остальные свойства
            Instrument instr = new Instrument(instance.Ticker, instance.TimeFrame);

            Portfolio portf = new Portfolio
            {
                Balance = instance.Balance,
                Rent = instance.Rent,
                Slippage = instance.Slippage
            };

            ICandleRepository candleRepository = new CandleRepository();

            TesterManager manager = new TesterManager(candleRepository, strategy, instr, portf);

            manager.DateFrom = new DateTime(2016, 6, 1);
            manager.DateTo = new DateTime(2016, 6, 2);

            //Стартуем стратегию
            manager.StartStrategy();

            //Проверить на количество ордеров
            Assert.AreEqual(strategy.Orders.Count, 2);

        }
    }
}