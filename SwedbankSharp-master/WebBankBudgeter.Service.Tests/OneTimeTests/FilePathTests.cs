using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebBankBudgeter.Service.Test;

namespace SwedbankSharpTests.OneTimeTests
{
    [TestClass]
    public class FilePathTests
    {
        [TestMethod]
        public void RelativPathStringTest()
        {
            var dir = TestData.DirPath;

            Assert.IsTrue(
                @"C:\Files\Dropbox\budget\Program\TestData\JsonTrans" == dir ||
                @"C:\Users\c_pergra\Dropbox\budget\Program\TestData\JsonTrans" == dir
            );
        }


    }
}
