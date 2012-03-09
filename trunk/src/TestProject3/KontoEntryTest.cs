﻿using Budgetterarn;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TestProject3
{
    
    
    /// <summary>
    ///This is a test class for KontoEntryTest and is intended
    ///to contain all KontoEntryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class KontoEntryTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for RowThatExists
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Budgetterarn.exe")]
        public void RowThatExistsTest()
        {
            KontoEntry_Accessor target = new KontoEntry_Accessor(); // TODO: Initialize to an appropriate value
            string[] inArray = new string[] { "test1", "223" };
            int columnNumber = 1; // TODO: Initialize to an appropriate value
            string expected = "223"; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.RowThatExists(inArray, columnNumber);
            Assert.AreEqual(expected, actual);
        }
    }
}