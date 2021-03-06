using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Collections.Generic;
using RMarket.ClassLib.Models;
using RMarket.ClassLib.Helpers;
using RMarket.ClassLib.Entities;
using RMarket.DataAccess.Helpers;

namespace RMarket.DataAccess.Context
{
    public partial class RMarketContext : DbContext
    {
        public RMarketContext()
            : base("name=RMarket")
        { }

        public virtual DbSet<Instance> Instances { get; set; }
        public virtual DbSet<StrategyInfo> StrategyInfoes { get; set; }
        public virtual DbSet<Ticker> Tickers { get; set; }
        public virtual DbSet<TimeFrame> TimeFrames { get; set; }
        public virtual DbSet<Candle> Candles { get; set; }
        public virtual DbSet<Selection> Selections { get; set; }
        public virtual DbSet<ConnectorInfo> ConnectorInfoes { get; set; }
        public virtual DbSet<DataProvider> DataProviders { get; set; }
        public virtual DbSet<AliveStrategy> AliveStrategies { get; set; }
        public virtual DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Instance>()
                .Property(e => e.Balance)
                .HasPrecision(19, 2);

            modelBuilder.Entity<Instance>()
                .Property(e => e.Slippage)
                .HasPrecision(19, 7);

            modelBuilder.Entity<Instance>()
                .Property(e => e.Rent)
                .HasPrecision(6, 4);

            modelBuilder.Entity<Candle>()
                .Property(e => e.DateOpen).HasColumnType("datetime2")
                .HasPrecision(0);

            modelBuilder.Entity<Candle>()
                .Property(e => e.OpenPrice)
                .HasPrecision(19, 7);

            modelBuilder.Entity<Candle>()
                .Property(e => e.HighPrice)
                .HasPrecision(19, 7);

            modelBuilder.Entity<Candle>()
                .Property(e => e.LowPrice)
                .HasPrecision(19, 7);

            modelBuilder.Entity<Candle>()
                .Property(e => e.ClosePrice)
                .HasPrecision(19, 7);

            modelBuilder.Entity<Candle>()
                .Property(e => e.CreateDate).HasColumnType("datetime2")
                .HasPrecision(3);

            modelBuilder.Entity<Selection>()
                .Property(e => e.Balance)
                .HasPrecision(19, 2);

            modelBuilder.Entity<Selection>()
                .Property(e => e.Slippage)
                .HasPrecision(19, 7);

            modelBuilder.Entity<Selection>()
                .Property(e => e.Rent)
                .HasPrecision(6, 4);

            modelBuilder.Entity<DataProvider>()
                .Property(e => e.CreateDate).HasColumnType("datetime2")
                .HasPrecision(3);

            modelBuilder.Entity<AliveStrategy>()
                .Property(e => e.CreateDate).HasColumnType("datetime2")
                .HasPrecision(3);

            modelBuilder.Entity<Order>()
                .Property(e => e.ExpirationDate).HasColumnType("datetime2")
                .HasPrecision(0);

            modelBuilder.Entity<Order>()
                .Property(e => e.DateOpen).HasColumnType("datetime2")
                .HasPrecision(0);

            modelBuilder.Entity<Order>()
                .Property(e => e.DateClose).HasColumnType("datetime2")
                .HasPrecision(0);

            modelBuilder.Entity<Order>()
                .Property(e => e.DateOpenUTC).HasColumnType("datetime2")
                .HasPrecision(3);

            modelBuilder.Entity<Order>()
                .Property(e => e.DateCloseUTC).HasColumnType("datetime2")
                .HasPrecision(3);

            modelBuilder.Entity<Order>()
                .Property(e => e.TakeProfit)
                .HasPrecision(19, 7);
            
            modelBuilder.Entity<Order>()
                .Property(e => e.StopLoss)
                .HasPrecision(19, 7);

            modelBuilder.Entity<Order>()
                .Property(e => e.PriceOpen)
                .HasPrecision(19, 7);

            modelBuilder.Entity<Order>()
                 .Property(e => e.PriceClose)
                 .HasPrecision(19, 7);

            modelBuilder.Entity<Order>()
                 .Property(e => e.Profit)
                 .HasPrecision(19, 2);

            //modelBuilder.Entity<Ticker>()
            //    .Property(e=>e.Code)
            //    .HasColumnAnnotation()
        }
    }

    public class RMarketInitializer : CreateDatabaseIfNotExists<RMarketContext>
    {
        protected override void Seed(RMarketContext context)
        {
            ContextInitializerHelper helper = new ContextInitializerHelper(context);

            helper.SeedTickers();

            helper.SeedTimeFrames();

            helper.SeedStrategyInfoes();

            helper.SeedConnectorInfoes();

            helper.SeedConnectorQuik();

        }
    }

 }
