﻿using System;

namespace Budgeter.Core.Entities
{
    public class InBudget
    {
        public string CategoryDescription { get; set; }
        public double BudgetValue { get; set; }
        public DateTime YearAndMonth { get; set; }
    }
}
