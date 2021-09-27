using Budgetterarn;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestBudgetterarn
{
    [TestClass]
    public class BudgeterTest
    {
        [TestMethod] // TODO: Fixa att ladda testets general settings fil etc
        [Ignore]
        //[DeploymentItem("Budgetterarn.exe")]
        public void AutoLoadEtceTest()
        {
            var target = new ProgramSettings();

            const bool expected = false;

            var actual = target.AutoLoadEtc;

            Assert.AreEqual(expected, actual);
        }
    }
}