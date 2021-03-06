﻿using RMarket.ClassLib.Abstract;
using RMarket.ClassLib.Abstract.IRepository;
using RMarket.ClassLib.Entities;
using RMarket.ClassLib.Helpers;
using RMarket.ClassLib.Infrastructure;
using RMarket.ClassLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RMarket.ClassLib.Managers
{
    public class TesterManager: IManager
    {
        public ICandleRepository candleRepository;//!!! = CurrentRepository.CandleRepository;
        private CancellationTokenSource cts;

        public TesterManager(IStrategy strategy, Instrument instr, Portfolio portf)
        {
            this.Strategy = strategy;
            this.Instr = instr;
            this.Portf = portf;
            this.AliveType = AliveType.Test;
            this.OrderSender = new OrderSender(this);

            Strategy.Instr = Instr;
            Strategy.Manager = this;
            Strategy.Orders = new List<Order>();

            Strategy.Initialize();
            StrategyHelper.SubscriptionToEventAsync(Strategy); //Подписываем на событие формирования свечи асинхронно индикаторы, которые найдем в стратегии.

            cts = new CancellationTokenSource();
        }

        #region IManager

        public IStrategy Strategy { get; set; }
        public Instrument Instr { get; set; }
        public Portfolio Portf { get; set; }
        /// <summary>
        /// NULL
        /// </summary>
        public IDataProvider Connector { get; set; }
        public bool IsStarted { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public AliveType AliveType { get; set; }
        public OrderSender OrderSender { get; set; }

        #region Events

        public event EventHandler StrategyStopped;

        #endregion

        public void StartStrategy()
        {
            IsStarted = true;

            IEnumerable<Candle> candles = from c in candleRepository.Get()
                                      where c.TickerId == Instr.Ticker.Id && c.TimeFrameId == Instr.TimeFrame.Id && c.DateOpen >= DateFrom && c.DateOpen < DateTo
                                      orderby c.DateOpen
                                      select c;

            TaskFactory factory = new TaskFactory(cts.Token);
            factory.StartNew(() =>
            {
                foreach (Candle candle in candles)
                {
                    if (cts.Token.IsCancellationRequested) //выход из потока по кнопке "Стоп"
                        return;

                    //вызвать событие формирования свечи
                    Instr.Candles.Insert(0, candle);
                    Instr.OnCreatedCandle();
                    Instr.OnCreatedCandleAsync();

                    Strategy.Begin();
                }
                IsStarted = false;
            }
            );

        }

        public void StopStrategy()
        {
            cts.Cancel();
            IsStarted = false;

            if (StrategyStopped != null)
                StrategyStopped(this, null);
        }

        public Order OrderSend(string tickerCode, OrderType orderType, int volume, decimal stoploss = 0, decimal takeprofit = 0, DateTime? expirationDate = null, string comment = "")
        {

            Order order = new Order();
            order.TickerCode = tickerCode;
            order.OrderType = orderType;
            order.Volume = volume;
            order.StopLoss = stoploss;
            order.TakeProfit = takeprofit;
            order.ExpirationDate = expirationDate;
            order.DateOpen = Instr.Candles[0].DateOpen;
            order.DateOpenUTC = DateTime.UtcNow;
            order.PriceOpen = Instr.Candles[0].ClosePrice + Portf.Slippage * ((order.OrderType == OrderType.Sell) ? (-1) : 1);

            decimal oldBalance = Portf.Balance;
            Portf.Balance = Math.Round(Portf.Balance - order.PriceOpen * order.Volume * (100 - Portf.Rent) / 100, 2);
            if (Portf.Balance < 0)
            {
                Portf.Balance = oldBalance;
                return null;
            }

            Strategy.Orders.Insert(0, order);

            return order;
        }

        public int OrderClose(Order order)
        {
            int res = 0;

            order.DateClose = Instr.Candles[0].DateOpen;
            order.DateCloseUTC = DateTime.UtcNow;

            order.PriceClose = Instr.Candles[0].ClosePrice;
            order.Profit = order.Volume * (order.PriceClose - order.PriceOpen) * ((order.OrderType == OrderType.Sell) ? (-1) : 1);

            Portf.Balance = Portf.Balance + order.PriceClose * order.Volume;

            return res;
        }

        #endregion

    }
}
