﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net;
using RMarket.ClassLib.Models;
using RMarket.ClassLib.Entities;

namespace RMarket.ClassLib.HistoricalProviders
{
    public class Finam : HistoricalProviderBase
    {
        public override IEnumerable<Candle> DownloadCandles(DateTime dateFrom, DateTime dateTo, Ticker ticker, TimeFrame timeFrame)
        {
            List<Candle> listCandles = new List<Candle>();

            //Пример: http://195.128.78.52/SBER_141008_141008.txt?market=1&em=3&code=SBER&df=8&mf=9&yf=2014&from=08.10.2014&dt=8&mt=9&yt=2014&to=08.10.2014&p=7&f=SBER_141008_141008&e=.txt&cn=SBER&dtf=1&tmf=1&MSOR=1&mstime=on&mstimever=1&sep=1&sep2=1&datf=1&at=1
            StringBuilder sb = new StringBuilder("http://195.128.78.52/SBER_141008_141008.txt?market=1");
            sb.Append("&em=" + ticker.CodeFinam); //Код тикера Финам
            sb.Append("&code=" + ticker.Code);
            sb.Append("&df=" + dateFrom.Day);
            sb.Append("&mf=" + (dateFrom.Month - 1));
            sb.Append("&yf=" + dateFrom.Year);
            sb.Append("&from=" + dateFrom.ToString("dd.MM.yyyy"));
            sb.Append("&dt=" + dateTo.Day);
            sb.Append("&mt=" + (dateTo.Month - 1));
            sb.Append("&yt=" + dateTo.Year);
            sb.Append("&to=" + dateTo.ToString("dd.MM.yyyy"));
            sb.Append("&p=" + timeFrame.CodeFinam); //Код интервала Финам
            sb.Append("&f=SBER_141008_141008"); //название выходного файла
            sb.Append("&e=.csv"); //.txt .csv
            sb.Append("&cn=" + ticker.Code);
            sb.Append("&dtf=1&tmf=1");
            sb.Append("&MSOR=0"); //0 - Время начало свечи, 1-окончания
            sb.Append("&mstime=on&mstimever=1&sep=1&sep2=1&datf=1&at=1");
            sb.Append("&fsp=1"); //Заполнять периоды без сделок
            WebRequest reqGet = WebRequest.Create(sb.ToString());

            WebResponse resp = reqGet.GetResponse();
            Stream stream = resp.GetResponseStream();
            StreamReader sr = new StreamReader(stream);

            if (sr.Peek() >= 0)
            {
                Dictionary<string, int> head = new Dictionary<string, int>();
                string strLine;
                int count = 0;

                while ((strLine = sr.ReadLine()) != null)
                {
                    string[] strArray = strLine.Split(new char[] { ',' });

                    count++;
                    if (count == 1) //Шапка
                    {
                        //Заполнить шапку
                        for (int i = 0; i < strArray.Length; i++)
                        {
                            //<TICKER>,<PER>,<DATE>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOL>
                            strArray[i] = Regex.Replace(strArray[i], "[<>]", "");
                            head.Add(strArray[i], i);
                        }
                        continue;
                    }

                    if (strArray.Length != head.Count)
                        continue; //бывают траблы(

                    //Сформировать свечу
                    DateTime dateOpen = DateTime.ParseExact(strArray[head["DATE"]] + strArray[head["TIME"]], "yyyyMMddHHmmss", null);
                    decimal OpenPrice = Convert.ToDecimal(strArray[head["OPEN"]], new CultureInfo("en-US"));
                    decimal HighPrice = Convert.ToDecimal(strArray[head["HIGH"]], new CultureInfo("en-US"));
                    decimal LowPrise = Convert.ToDecimal(strArray[head["LOW"]], new CultureInfo("en-US"));
                    decimal ClosePrice = Convert.ToDecimal(strArray[head["CLOSE"]], new CultureInfo("en-US"));
                    int Volume = Convert.ToInt32(strArray[head["VOL"]]);

                    //!!!Создает ticker и timeFrame                
                    Candle candle = new Candle(ticker.Id, timeFrame.Id, dateOpen, OpenPrice, HighPrice, LowPrise, ClosePrice, Volume);

                    listCandles.Add(candle);
                }
            }

            return listCandles;

        }

    }
}
