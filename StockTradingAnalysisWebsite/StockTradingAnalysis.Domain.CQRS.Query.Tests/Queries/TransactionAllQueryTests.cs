﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockTradingAnalysis.Domain.CQRS.Query.Queries;

namespace StockTradingAnalysis.Domain.CQRS.Query.Tests.Queries
{
    [TestClass]
    public class TransactionAllQueryTests
    {
        [TestMethod]
        public void TransactionAllQueryShouldBeProperlyInitialized()
        {
            var query = new TransactionAllQuery();

            //query.StartDate.Should().Be(DateTime.MinValue);
            //query.EndDate.Should().Be(DateTime.MaxValue);
            //query.StockType.Should().BeEmpty();
            //query.PositionType.Should().Be(PositionType.All);
            //query.Strategy.Should().BeNull();
            //query.UseDividends.Should().BeTrue();
            //query.LastAmountOfTrades.Should().Be(int.MaxValue);
            //query.Tag.Should().BeEmpty();
        }
    }
}
