using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PdfToText;
using Budgeter.Core.Entities;
using System.Collections.Generic;
using Budgetterarn.DAL;

namespace UtilitiesTest
{
    [TestClass]
    public class PfReadTests
    {
        [TestMethod]
        public void ReadPdfTest()
        {
            // Arrange
            var fileFullPath =
                @"C:\Files\Dropbox\budget\Program\TestData\Allkortsfaktura 629 011 192 Oktober 2015.pdf"
                ;
            var expected = "3058";

            // Act
            var text = QuickReadPdf.ReadPdf(fileFullPath);

            // Assert
            Assert.IsNotNull(text);
            //Assert.AreEqual(expected, text);
        }

        [TestMethod]
        public void ParseToKontoEntriesFromRedPdfTest()
        {
            var expected = 49;


            // Arrange
            var fileFullPath =
                @"C:\Files\Dropbox\budget\Program\TestData\Allkortsfaktura 629 011 192 Oktober 2015.pdf"
                ;
            var tearget = new KontoFromPdfParser(fileFullPath);

            // Act
            var results = tearget.ParseToKontoEntriesFromRedPdf();

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(expected, results.Count);
        }
    }
}
