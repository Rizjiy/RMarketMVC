﻿using RMarket.ClassLib.Abstract;
using RMarket.ClassLib.Entities;
using RMarket.ClassLib.EntityModels;
using RMarket.ClassLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMarket.UnitTests.Infrastructure.Strategies
{
    public class StrategyMock1: IStrategy
    {
        public Instrument Instr { get; set; }
        public IManager Manager { get; set; }
        public List<Order> Orders { get; set; }

        public void Initialize()
        {
        }

        public void Begin()
        {
            //покупаем и продаем через каждые 20 свечек
            if (Instr.Candles.Count % 20 == 0)
            {
                Manager.OrderSender.OrderCloseAll(OrderType.Buy);
                Manager.OrderSender.OrderCloseAll(OrderType.Sell);

                if ((Instr.Candles.Count / 20) % 2 != 0)
                {
                    Manager.OrderSender.OrderBuy(1);
                }
                else
                {
                    Manager.OrderSender.OrderSell(1);
                }

            }
        }

        public void OnTickPoked(object sender, TickEventArgs e)
        {

        }

        public static InstanceModel GetModel()
        {
            EntityInfo entityInfo = new EntityInfo
            {
                Id = 1,
                Name = "Strategy Mosk",
                EntityType = EntityType.StrategyInfo,
                TypeName = typeof(StrategyMock1).AssemblyQualifiedName
            };

            Ticker ticker = new Ticker { Id = 1, Name = "SBER", Code = "SBER", QtyInLot = 10 };
            TimeFrame timeFrame = new TimeFrame { Id = 4, Name = "10", ToMinute = 10 };

            InstanceModel instance = new InstanceModel
            {
                Id = 1,
                Name = "test instance1",
                EntityInfoId = entityInfo.Id,
                EntityInfo = entityInfo,
                TickerId = ticker.Id,
                Ticker = ticker,
                TimeFrameId = timeFrame.Id,
                TimeFrame = timeFrame,
                Balance = 1000,
                Slippage = 0,
                Rent = 0,
                GroupID = Guid.NewGuid(),
                CreateDate = new DateTime(2016, 01, 01),
            };

            return instance;
        }


    }
}
