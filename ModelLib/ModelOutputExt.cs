﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLib
{
    public class ModelOutputExt
    {
        public float ForecastedSales { get; set; }
        public float LowerBoundSales { get; set; }
        public float UpperBoundSales { get; set; }
        public string SalesDate { get; set; }
        public float TotalSales { get; set; }
        public int indexX { get; set; }
    }
}
