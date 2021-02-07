using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SwedbankSharpTests.OneTimeTests
{
    [TestClass]
    public class DateTests
    {
        [TestMethod]
        public void MonthToStringTest()
        {
            var testDate = new DateTime(2020, 4, 1);

            Assert.AreEqual("april",
                testDate.ToString("MMMM")
            );

            Assert.AreEqual("september",
                testDate.AddMonths(5).ToString("MMMM")
            );

            Assert.AreEqual("5",
                5.15.ToString("N0")
            );
        }
    }
}
