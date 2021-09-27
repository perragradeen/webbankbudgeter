using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Budgeter.Core;
using Budgeter.Core.Entities;
using Budgetterarn.EntryLogicSetFlags;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestBudgetterarn
{
    [TestClass]
    public class CheckNewEntriesTests
    {
        private static List<KontoEntry> TestDataKoList =>
            (new List<KontoEntry>
            {
                new KontoEntry
                {
                    Date = DateTime.Now.AddDays(-2), KostnadEllerInkomst = 1, TypAvKostnad = "hemförsäkring",
                    Info = "testinkomst"
                },
                new KontoEntry
                {
                    Date = DateTime.Now.AddDays(-1), KostnadEllerInkomst = 2, TypAvKostnad = "hemförsäkring",
                    Info = "testinkomst"
                },
                new KontoEntry
                {
                    Date = DateTime.Now.AddDays(-3), KostnadEllerInkomst = 3, TypAvKostnad = "hemförsäkring",
                    Info = "testinkomst"
                },
            });

        private static KontoEntriesViewModelListUpdater TestDataGet
        {
            get
            {
                var newKos = new SortedList();
                TestDataKoList.ForEach(e => newKos.Add(e.KeyForThis, e));

                return new KontoEntriesViewModelListUpdater
                {
                    KontoEntries = new SortedList(new DescendingComparer()),
                    NewItemsListEdited =
                        TestDataKoList,
                    NewKontoEntriesIn = newKos,
                };
            }
        }

        [TestMethod]
        public void Check_If_New_Entries_Adds_Up_Test()
        {
            // if theese preconditions exists
            var testData = TestDataGet;
            var target = new KontoEntriesChecker(testData);

            // If begin value is
            var inCount = testData.NewKontoEntriesIn.Count;

            // When this happens
            target.CheckAndAddNewItemsForLists();

            // Then it sholud be
            var afterCount = testData.NewItemsListEdited.Count;
            Assert.AreEqual(inCount, afterCount);
        }

        [TestMethod]
        public void Check_If_New_Entries_Do_Not_add_doubles_when_it_Adds_Up_Test()
        {
            // if theese preconditions exists
            var testData = TestDataGet;
            var oldData = TestDataGet;
            oldData.NewItemsListEdited.First().KostnadEllerInkomst = 9123;
            testData.NewItemsListEdited = oldData.NewItemsListEdited;
            var target = new KontoEntriesChecker(testData);

            // If begin value is
            var inCount = testData.NewKontoEntriesIn.Count;

            // When this happens
            target.CheckAndAddNewItemsForLists();

            // Then it sholud be
            var afterCount = testData.NewItemsListEdited.Count;
            Assert.AreEqual(inCount + 1, afterCount);
        }

        [TestMethod]
        public void Check_If_New_Entries_Adds_new_Color_Test()
        {
            // if theese preconditions exists
            var testData = TestDataGet;

            var oldData = TestDataGet;
            oldData.NewItemsListEdited.First().Info = "annan info utan 0-or";
            testData.NewItemsListEdited = oldData.NewItemsListEdited;

            TestDataKoList.ForEach(x => testData.KontoEntries.Add(x.KeyForThis, x));
            var target = new KontoEntriesChecker(testData);

            // If begin value is
            var inCount = testData.NewKontoEntriesIn.Count;

            // When this happens
            target.CheckAndAddNewItemsForLists();

            // Then it sholud be
            var afterCount = testData.NewItemsListEdited.Count;
            Assert.AreEqual(inCount, afterCount);

            var old1 = testData.NewItemsListEdited.First();
            foreach (var entry in testData.NewItemsListEdited)
            {
                // BudgeterForm
                // kolla om det är "Skyddat belopp", och se om det finns några gamla som matchar.
                SkyddatBeloppChecker.CheckForSkyddatBeloppMatcherAndGuessDouble(entry, testData.KontoEntries);
            }

            var new1 = testData.NewItemsListEdited.First();
            Assert.AreEqual(old1.FontFrontColor, new1.FontFrontColor);
        }
    }
}