﻿using RMarket.ClassLib.Abstract;
using RMarket.ClassLib.Entities;
using RMarket.ClassLib.Helpers;
using RMarket.ClassLib.Models;
using RMarket.Concrete.DataProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMarker.Concrete.DataProviders.Infrastructure
{
    public class DataProvidersContextInicializer: IContextInitializer<DataProviderSetting>
    {
        public IEnumerable<DataProviderSetting> Get()
        {
            List<DataProviderSetting> res = new List<DataProviderSetting>();

            res.Add(GetQuik());
            res.Add(GetCsv());

            return res;
        }

        private DataProviderSetting GetQuik()
        {
            List<ParamEntity> entityParams = new List<ParamEntity>
            {
                new ParamEntity {FieldName="ServerName",FieldValue="RMarket" },
                new ParamEntity {FieldName="Col_Date",FieldValue="Дата" },
                new ParamEntity {FieldName="Col_Time",FieldValue="Время" },
                new ParamEntity {FieldName="Col_TickerCode",FieldValue="Код бумаги" },
                new ParamEntity {FieldName="Col_Price",FieldValue="Цена" },
                new ParamEntity {FieldName="Col_Qty",FieldValue="Кол-во" },
                new ParamEntity {FieldName="Col_Period",FieldValue="Период" }
            };

            return new DataProviderSetting
            {
                Name = "Quik default",
                CreateDate = DateTime.Now,
                Description = "русскоязычные настройки квика",
                StrParams = Serializer.Serialize(entityParams),
                EntityInfo = new EntityInfo
                {
                    Name = "QuikProvider",
                    TypeName = typeof(QuikProvider).AssemblyQualifiedName,
                    EntityType = EntityType.DataProviderInfo
                }
            };

        }

        private DataProviderSetting GetCsv()
        {
            List<ParamEntity> entityParams = new List<ParamEntity>
            {
                new ParamEntity {FieldName="FilePath",FieldValue=@"C:\Projects\RMarketMVCgit\RMarketMVC\RMarket.UnitTests\Infrastructure\files\SBER_160601_160601.csv" },
                new ParamEntity {FieldName="Separator",FieldValue=";" },
                new ParamEntity {FieldName="Col_Date",FieldValue="<DATE>" },
                new ParamEntity {FieldName="FormatDate",FieldValue="yyyyMMdd" },
                new ParamEntity {FieldName="Col_Time",FieldValue="<TIME>" },
                new ParamEntity {FieldName="FormatTime",FieldValue="HHmmss" },
                new ParamEntity {FieldName="Col_TickerCode",FieldValue="<TICKER>" },
                new ParamEntity {FieldName="Col_Price",FieldValue="<LAST>" },
                new ParamEntity {FieldName="Col_Volume",FieldValue="<VOL>" },
                new ParamEntity {FieldName="Val_SessionStart",FieldValue="10:00:00" },
                new ParamEntity {FieldName="Val_SessionFinish",FieldValue="19:00:00" },
                new ParamEntity {FieldName="Delay",FieldValue="600" }
            };

            return new DataProviderSetting
            {
                Name = "csv Finam",
                EntityInfoId = 2,
                CreateDate = DateTime.Now,
                Description = "Настройка для .csv от Финама",
                StrParams = Serializer.Serialize(entityParams),
                EntityInfo = new EntityInfo
                {
                    Name = "CsvFileProvider",
                    TypeName = typeof(CsvFileProvider).AssemblyQualifiedName,
                    EntityType = EntityType.DataProviderInfo
                }
            };
            

        }

    }
}