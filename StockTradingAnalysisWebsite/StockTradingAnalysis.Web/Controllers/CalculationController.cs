﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using AutoMapper;
using StockTradingAnalysis.Domain.CQRS.Cmd.Commands;
using StockTradingAnalysis.Domain.CQRS.Cmd.Exceptions;
using StockTradingAnalysis.Domain.CQRS.Query.Queries;
using StockTradingAnalysis.Interfaces.Commands;
using StockTradingAnalysis.Interfaces.Domain;
using StockTradingAnalysis.Interfaces.Queries;
using StockTradingAnalysis.Interfaces.Services;
using StockTradingAnalysis.Interfaces.Services.Domain;
using StockTradingAnalysis.Web.Models;

namespace StockTradingAnalysis.Web.Controllers
{
    public class CalculationController : Controller
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IPriceCalculatorService _priceCalculatorService;

        public CalculationController(
            IQueryDispatcher queryDispatcher,
            ICommandDispatcher commandDispatcher,
            IPriceCalculatorService priceCalculatorService)
        {
            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;
            _priceCalculatorService = priceCalculatorService;
        }

        // GET: Calculation
        public ActionResult Index()
        {
            return
                View(Mapper.Map<IEnumerable<CalculationViewModel>>(_queryDispatcher.Execute(new CalculationAllQuery())));
        }

        // GET: /Calculation/Copy/5
        public ActionResult Copy(Guid id, CalculationViewModel model)
        {
            _commandDispatcher.Execute(new CalculationCopyCommand(Guid.NewGuid(), id, model.OriginalVersion));

            return RedirectToAction("Index");
        }

        // GET: Calculation/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Calculation/Create
        [HttpPost]
        public ActionResult Create(CalculationViewModel model)
        {
            var id = Guid.NewGuid();

            try
            {
                _commandDispatcher.Execute(new CalculationAddCommand(id, model.OriginalVersion, model.Name, model.Wkn,
                    model.Multiplier, model.StrikePrice, model.Underlying, model.InitialSl,
                    model.InitialTp, model.PricePerUnit, model.OrderCosts, model.Description, model.Units, model.IsLong));

                return RedirectToAction("Index");
            }
            catch (DomainValidationException validationException)
            {
                ModelState.AddModelError(validationException.Property, validationException.Message);
            }

            return View(model);
        }

        // GET: Calculation/Edit/5
        public ActionResult Edit(Guid id)
        {
            var model = Mapper.Map<CalculationViewModel>(_queryDispatcher.Execute(new CalculationByIdQuery(id)));
            ViewBag.CalculationResult = Mapper.Map<CalculationResultViewModel>(
                _priceCalculatorService.CalculatePotentialEarnings((ICalculation)model));

            return View(model);
        }

        // POST: Calculation/Edit/5
        [HttpPost]
        public ActionResult Edit(Guid id, CalculationViewModel model)
        {
            try
            {
                _commandDispatcher.Execute(new CalculationChangeCommand(id, model.OriginalVersion, model.Name, model.Wkn,
                    model.Multiplier, model.StrikePrice, model.Underlying, model.InitialSl,
                    model.InitialTp, model.PricePerUnit, model.OrderCosts, model.Description, model.Units, model.IsLong));

                return RedirectToAction("Index");
            }
            catch (DomainValidationException validationException)
            {
                ModelState.AddModelError(validationException.Property, validationException.Message);
            }

            return View(model);
        }

        // GET: Calculation/Delete/5
        public ActionResult Delete(Guid id)
        {
            var model = Mapper.Map<CalculationViewModel>(_queryDispatcher.Execute(new CalculationByIdQuery(id)));
            ViewBag.CalculationResult = Mapper.Map<CalculationResultViewModel>(
                _priceCalculatorService.CalculatePotentialEarnings(Mapper.Map<ICalculation>(model)));

            return View(model);
        }

        // POST: Calculation/Delete/5
        [HttpPost]
        public ActionResult Delete(Guid id, CalculationViewModel model)
        {
            try
            {
                _commandDispatcher.Execute(new CalculationRemoveCommand(id, model.OriginalVersion));

                return RedirectToAction("Index");
            }
            catch (DomainValidationException validationException)
            {
                ModelState.AddModelError(validationException.Property, validationException.Message);
            }

            return View(model);
        }

        /// <summary>
        /// Calculates the price of a hedged certificate
        /// </summary>
        /// <param name="underlyingPrice">Current price of the underlying</param>
        /// <param name="multiplier">Multiplier</param>
        /// <param name="strikePrice">Strike price</param>
        /// <param name="isLong">Long or short</param>
        /// <returns>Underlying Price</returns>
        public JsonResult CalculatePriceFromUnderlying(decimal? underlyingPrice, decimal? multiplier,
            decimal? strikePrice, bool? isLong)
        {
            var price = _priceCalculatorService.CalculatePriceFromUnderlying(underlyingPrice, multiplier, strikePrice,
                isLong);

            return Json(price, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returns the underlying price based on the given parameters
        /// </summary>
        /// <param name="model">The calculation model</param>
        /// <returns></returns>
        public JsonResult GetStatistics(CalculationViewModel model)
        {
            var result =
                Mapper.Map<CalculationResultViewModel>(
                    _priceCalculatorService.CalculatePotentialEarnings(Mapper.Map<ICalculation>(model)));

            return Json(new
            {
                Result = RenderPartialViewToString("ViewModelCalculationResult", result)
            });
        }

        /// <summary>
        /// Returns a rendered string from the given model using a partial view
        /// </summary>
        /// <param name="viewName">View</param>
        /// <param name="model">Model</param>
        /// <returns>Rendered HTMl code</returns>
        private string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
    }
}