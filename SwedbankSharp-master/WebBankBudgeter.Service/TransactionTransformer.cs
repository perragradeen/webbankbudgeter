﻿using Budgeter.Core.Entities;
using System;
using System.Collections.Generic;
using WebBankBudgeter.Service.Model;
// ReSharper disable IdentifierTypo

namespace WebBankBudgeter.Service
{
    internal class TransactionTransformer
    {
        /// <summary>
        /// Transaction row from file
        /// </summary>
        private readonly KontoEntry _kontoEnry;
        private readonly Func<string, string> lookUpCategoryGroup;

        public TransactionTransformer(KontoEntry kontoEnry, Func<string, string> lookUpCategoryGroup)
        {
            _kontoEnry = kontoEnry;
            this.lookUpCategoryGroup = lookUpCategoryGroup;
        }

        public Transaction GetTransaction()
        {
            var transaction = new Transaction
            {
                DateAsDate = _kontoEnry.Date,
                Description = _kontoEnry.Info,
                AmountAsDouble = _kontoEnry.KostnadEllerInkomst,
                Categorizations = new Categorizations
                {
                    Categories = new List<Categories> {
                        new Categories {
                            Group = GetCategoryGroup(_kontoEnry.TypAvKostnad),
                            Name = _kontoEnry.TypAvKostnad
                        }
                    }
                }
            };
            return transaction;
        }

        private string GetCategoryGroup(string typAvKostnad)
        {
            //return "ID_OTHER";

            var categoryGroup = lookUpCategoryGroup(typAvKostnad);
            return categoryGroup;

            // valde 1.
            // 1. Slå upp via xml här
            // 2. Ha med och välj i budgeterarn
            // 3. Hårdkoda uppslag
        }
    }
}