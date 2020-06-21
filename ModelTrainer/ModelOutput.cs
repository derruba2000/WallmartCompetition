using System;
using System.Collections.Generic;
using System.Text;

namespace ModelTrainer
{
    class ModelOutput
    {
        public float[] ForecastedSales { get; set; }

        public float[] LowerBoundSales { get; set; }

        public float[] UpperBoundSales { get; set; }
    }
}
