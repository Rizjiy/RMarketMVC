﻿using Newtonsoft.Json;
using RMarket.ClassLib.Abstract;
using RMarket.ClassLib.Connectors;
using RMarket.ClassLib.Entities;
using RMarket.ClassLib.EntityModels;
using RMarket.ClassLib.Helpers;
using RMarket.ClassLib.Managers;
using RMarket.ClassLib.Models;
using RMarket.WebUI.Infrastructure;
using RMarket.WebUI.Infrastructure.Helpers;
using RMarket.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RMarket.WebUI.Controllers
{
    public class EmulController : Controller
    {
        private IInstanceRepository instanceRepository;
        private ISettingRepository settingRepository;
        private IAliveStrategyRepository aliveStrategyRepository;
        private JsonSerializerSettings jsonSerializerSettings;

        List<AliveResult> strategyResultCollection = CurrentUI.AliveResults;

        public EmulController(IInstanceRepository instanceRepository, ISettingRepository settingRepository, IAliveStrategyRepository aliveStrategyRepository)
        {
            this.instanceRepository = instanceRepository;
            this.settingRepository = settingRepository;
            this.aliveStrategyRepository = aliveStrategyRepository;

            jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;

        }

        // GET:
        public ActionResult Index()
        {
            //определить наличия работающего теста и вызвать форму нового, или результата
            if (strategyResultCollection.Count == 0)
            {
                DateTime dateFrom = DateTime.Now.Date.AddMonths(-1);
                DateTime dateTo = DateTime.Now.Date;
                return RedirectToAction("BeginTest", new { instanceId = 0, dateFrom = dateFrom, dateTo = dateTo });
            }
            else
            {
                return View(strategyResultCollection);
            }
        }

        [HttpGet]
        public ViewResult BeginTest()
        {
            ViewBag.InstanceList = ModelHelper.GetInstanceList(instanceRepository);
            ViewBag.SettingList = ModelHelper.GetSettingList(settingRepository, SettingType.ConnectorInfo);

            DateTime dateFrom = DateTime.Now.Date.AddMonths(-1);
            DateTime dateTo = DateTime.Now.Date;

            TesterModel model = new TesterModel();
            model.DateFrom = dateFrom;
            model.DateTo = dateTo;

            return View(model);
        }

        [HttpPost]
        public ActionResult BeginTest(TesterModel model) //!!!В модели использовать Instance
        {
            if (ModelState.IsValid)
            {
                InstanceModel instance = instanceRepository.GetById(model.InstanceId, true);
                Setting setting = settingRepository.Find(model.SettingId);

                //добавляем живую стратегию
                AliveStrategy aliveStrategy = new AliveStrategy
                {
                    GroupID = instance.GroupID,
                    IsActive = true
                };
                aliveStrategyRepository.Save(aliveStrategy);

                //получаем стратегию 
                IStrategy strategy = StrategyHelper.CreateStrategy(instance);

                //устанавливаем остальные свойства
                Instrument instr = new Instrument(instance.Ticker, instance.TimeFrame);

                Portfolio portf = new Portfolio
                {
                    Balance = instance.Balance,
                    Rent = instance.Rent,
                    Slippage = instance.Slippage
                };

                IDataProvider connector = SettingHelper.CreateDataProvider(setting);
                //IDataProvider connector = (IDataProvider)ReflectionHelper.CreateEntity(setting);

                IManager manager = new EmulManager(strategy, instr, portf, connector, aliveStrategy);

                //manager.DateFrom = model.DateFrom;
                //manager.DateTo = model.DateTo;

                //Стартуем стратегию
                manager.StartStrategy();

                //***Положить в сессию
                AliveResult testResult = new AliveResult
                {
                    AliveId = aliveStrategy.Id,
                    StartDate = DateTime.Now,
                    Instance = instance,
                    Strategy = strategy,
                    Manager = manager
                };

                //Извлечь индикаторы из  объекта стратегии
                testResult.IndicatorsDict = StrategyHelper.ExtractIndicatorsInStrategy(strategy);

                strategyResultCollection.Add(testResult);
                //**

                TempData["message"] = string.Format("Эмуляция успешно запущена. Id={0}", testResult.AliveId);

                return RedirectToAction("Index");
            }

            ViewBag.InstanceList = ModelHelper.GetInstanceList(instanceRepository);

            return View(model);
        }

        [HttpGet]
        public ActionResult DisplayResult(int aliveId)
        {
            AliveResult testResult = strategyResultCollection.FirstOrDefault(t => t.AliveId == aliveId);
            if (testResult == null)
            {
                TempData["error"] = string.Format("Не найдена эмуляция Id:{0}", aliveId);
                return RedirectToAction("Index");
            }

            //if (testResult.Strategy.Instr.Candles.Count == 0)
            //{
            //    TempData["error"] = string.Format("Нет сформированных свечей за выбранный период Id:{0}", aliveId);
            //    return RedirectToAction("Index");
            //}

            return View(testResult);

        }

        [HttpGet]
        public RedirectToRouteResult TerminateTest(int aliveId)
        {
            AliveResult testResult = strategyResultCollection.FirstOrDefault(t => t.AliveId == aliveId);
            if (testResult != null && testResult.Manager.IsStarted)
            {
                testResult.Manager.StopStrategy();
                TempData["warning"] = string.Format("Эмуляция Id={0} была прервана!", testResult.AliveId);
            }

            return RedirectToAction("DisplayResult", new { aliveId = testResult.AliveId });
        }


        #region //////////////////////Навигация по графику
        /// <summary>
        /// Загрузка последних n свечек
        /// </summary>
        /// <param name="aliveId"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public ActionResult GetDataJsonInit(int aliveId, int maxCount, string way = "right")
        {
            AliveResult testResult = strategyResultCollection.FirstOrDefault(t => t.AliveId == aliveId);

            AliveResultHelperUI helper = new AliveResultHelperUI(testResult);
            var res = helper.GetDataJsonInit(maxCount, way);

            return new JsonNetResult(res, JsonRequestBehavior.AllowGet, jsonSerializerSettings);
        }

        /// <summary>
        /// загрузка свечей после определенной даты вперед-назад
        /// </summary>
        /// <param name="aliveId"></param>
        /// <param name="lastDateUTC"></param>
        /// <param name="maxCount"></param>
        /// /// <param name="way">right, left</param>
        /// <returns></returns>
        public ActionResult GetDataJsonSlice(int aliveId, double lastDateUTC, int maxCount, string way = "right")
        {
            DateTime posixTime = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
            DateTime lastDate = posixTime.AddMilliseconds(lastDateUTC);

            AliveResult testResult = strategyResultCollection.FirstOrDefault(t => t.AliveId == aliveId);
            if (testResult == null)
            {
                TempData["error"] = string.Format("Не найден тест Id={0}", testResult.AliveId);
                return RedirectToAction("Index");
            }

            AliveResultHelperUI helper = new AliveResultHelperUI(testResult);
            var res = helper.GetDataJsonSlice(lastDate, maxCount, way);

            return new JsonNetResult(res, JsonRequestBehavior.AllowGet, jsonSerializerSettings);
        }

        /// <summary>
        /// подгрузка свечей через интервал
        /// </summary>
        /// <param name="aliveId"></param>
        /// <param name="lastDateUTC"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public ActionResult GetDataJsonAdd(int aliveId, double lastDateUTC, int maxCount)
        {
            DateTime posixTime = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
            DateTime lastDate = posixTime.AddMilliseconds(lastDateUTC);

            AliveResult testResult = strategyResultCollection.FirstOrDefault(t => t.AliveId == aliveId);
            if (testResult == null)
            {
                TempData["error"] = string.Format("Не найден тест Id={0}", testResult.AliveId);
                return RedirectToAction("Index");
            }

            AliveResultHelperUI helper = new AliveResultHelperUI(testResult);
            var res = helper.GetDataJsonAdd(lastDate, maxCount);

            return new JsonNetResult(res, JsonRequestBehavior.AllowGet, jsonSerializerSettings);
        }
        #endregion
    }
}