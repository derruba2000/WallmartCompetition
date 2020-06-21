using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLib
{
    class ModelOutput
    {
        public float[] ForecastedSales { get; set; }

        public float[] LowerBoundSales { get; set; }

        public float[] UpperBoundSales { get; set; }
    }
}
