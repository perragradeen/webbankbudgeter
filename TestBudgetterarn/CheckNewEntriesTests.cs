using Budgeter.Core;
using Budgeter.Core.Entities;
using Budgetterarn;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TestBudgetterarn
{
    [TestClass]
    public class CheckNewEntriesTests
    {
        public List<KontoEntry> TestDataKoList
        {
            get
            {
                return (new List<KontoEntry>
                    {
                        new KontoEntry { Date = DateTime.Now.AddDays(-2), KostnadEllerInkomst = 1, TypAvKostnad = "hemförsäkring", Info = "testinkomst"},
                        new KontoEntry { Date = DateTime.Now.AddDays(-1), KostnadEllerInkomst = 2, TypAvKostnad = "hemförsäkring", Info = "testinkomst"},
                        new KontoEntry { Date = DateTime.Now.AddDays(-3), KostnadEllerInkomst = 3, TypAvKostnad = "hemförsäkring", Info = "testinkomst"},
                    });
            }
        }

        private KontoEntriesViewModelListUpdater TestDataGet
        {
            get
            {
                var newKos = new SortedList();
                TestDataKoList.ForEach(e => newKos.Add(e.KeyForThis, e));

                return new KontoEntriesViewModelListUpdater
                {
                    KontoEntries = new SortedList(new DescendingComparer()),
                    NewIitemsListEdited =
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

            // If begin value is
            var inCount = testData.NewKontoEntriesIn.Count;

            // When this happens
            KontoEntriesChecker.CheckAndAddNewItemsForLists(testData);

            // Then it sholud be
            var afterCount = testData.NewIitemsListEdited.Count;
            Assert.AreEqual(inCount, afterCount);
        }

        [TestMethod]
        public void Check_If_New_Entries_Do_Not_add_doubles_when_it_Adds_Up_Test()
        {
            // if theese preconditions exists
            var testData = TestDataGet;
            var oldData = TestDataGet;
            oldData.NewIitemsListEdited.First().KostnadEllerInkomst = 9123;
            testData.NewIitemsListEdited = oldData.NewIitemsListEdited;

            // If begin value is
            var inCount = testData.NewKontoEntriesIn.Count;

            // When this happens
            KontoEntriesChecker.CheckAndAddNewItemsForLists(testData);

            // Then it sholud be
            var afterCount = testData.NewIitemsListEdited.Count;
            Assert.AreEqual(inCount + 1, afterCount);
        }

        [TestMethod]
        public void Check_If_New_Entries_Adds_new_Color_Test()
        {
            // if theese preconditions exists
            var testData = TestDataGet;

            var oldData = TestDataGet;
            oldData.NewIitemsListEdited.First().Info = "annan info utan 0-or";
            testData.NewIitemsListEdited = oldData.NewIitemsListEdited;

            TestDataKoList.ForEach(x => testData.KontoEntries.Add(x.KeyForThis, x));

            // If begin value is
            var inCount = testData.NewKontoEntriesIn.Count;

            // When this happens
            KontoEntriesChecker.CheckAndAddNewItemsForLists(testData);

            // Then it sholud be
            var afterCount = testData.NewIitemsListEdited.Count;
            Assert.AreEqual(inCount, afterCount);

            var old1 = testData.NewIitemsListEdited.First();
            foreach (var entry in testData.NewIitemsListEdited)
            {
                // BudgeterForm
                // kolla om det är "Skyddat belopp", och se om det finns några gamla som matchar.
                KontoEntriesChecker.CheckForSkyddatBeloppMatcherAndGuesseDouble(entry, testData.KontoEntries);
            }

            var new1 = testData.NewIitemsListEdited.First();
            Assert.AreEqual(old1.FontFrontColor, new1.FontFrontColor);
        }
    }
}
