﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RMarket.ClassLib.Entities
{
    public enum OrderType
    {
        Buy=1,
        Sell=2
    }

    public class Order
    {
        public int Id { get; set; }
        public string TickerCode { get; set; }
        public OrderType OrderType { get; set; }
        
        /// <summary>
        /// 0 - без ограничений
        /// </summary>
        public decimal TakeProfit { get; set; }
        /// <summary>
        /// 0 - без ограничений
        /// </summary>
        public decimal StopLoss { get; set; }

        public DateTime DateOpenCandle { get; set; }
        public DateTime DateOpenUTS { get; set; }
        public DateTime Expiration { get; set; }
        public DateTime DateCloseCandle { get; set; }
        public DateTime DateCloseUTS { get; set; }

        public int Volume { get; set; }
        public decimal PriceOpen { get; set; }
        public decimal PriceClose { get; set; }
        public decimal Profit { get; set; }

        public string Comment { get; set; }

        public int AliveStrategyId { get; set; }

        public AliveStrategy AliveStrategy { get; set; }


    }
}