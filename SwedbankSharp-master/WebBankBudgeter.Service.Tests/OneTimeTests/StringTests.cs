using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwedbankSharp.JsonSchemas;
using WebBankBudgeter.Service.Services;

namespace SwedbankSharpTests.OneTimeTests
{
    [TestClass]
    public class StringTests
    {
        [TestMethod]
        public void ExpenseControlIncludedAlternativesTest()
        {
            var trans = new Transaction
            {
                ExpenseControlIncluded = "OUTDATED"
            };

            Assert.AreEqual(ExpenseControlIncludedAlternatives.OUTDATED,
                trans.ExpenseControlIncludedAsEnum
            );

            trans = new Transaction
            {
                ExpenseControlIncluded = "INCLUDED"
            };

            Assert.AreEqual(ExpenseControlIncludedAlternatives.INCLUDED,
                trans.ExpenseControlIncludedAsEnum
            );

        }

        [TestMethod]
        public void DoubleParseTest()
        {
            const string s = "4.54";
            const string s2 = "4,54";
            const string s3 = "0,54";

            Assert.AreEqual(4.54, Conversions.DoubleParseAdvanced(s));
            Assert.AreEqual(4.54, Conversions.DoubleParseAdvanced(s2));
            Assert.AreEqual(0.54, Conversions.DoubleParseAdvanced(s3));
        }
    }
}
