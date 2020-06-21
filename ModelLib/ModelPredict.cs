using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML;
using System.IO;
using Microsoft.ML.Data;
using Microsoft.ML.TimeSeries;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using Microsoft.ML.Transforms.TimeSeries;

namespace ModelLib
{
    public class ModelPredict
    {
        string connectionstring;
        string modelPath;
        string product;
        ITransformer model;
        MLContext ctx;
        IDataView DataExtracted;

        public ModelPredict(string connectionstringRef, string ModelsPathRef, string productRef)
        {
            this.ctx = new MLContext();
            this.connectionstring = connectionstringRef;
            this.modelPath = ModelsPathRef;
            this.product = productRef;
            if (product.Contains("evaluation"))
            {
                this.modelPath = ModelsPathRef + "\\evaluation\\" + $"Model_{productRef}.zip";
            }
            else
            {
                this.modelPath = ModelsPathRef + "\\validation\\" + $"Model_{productRef}.zip";
            }
            using (var file = File.OpenRead(modelPath))
                this.model = ctx.Model.Load(file, out DataViewSchema schema);
        }


        public string ExtractData(int PreviousDaysRef, int FutureDaysRef, string CutOffDateRef)
        {
            string QuerySTR = @"
                DECLARE @FutureDays INT  = "+FutureDaysRef.ToString()+@";
                DECLARE @PreviousDays INT  = "+PreviousDaysRef.ToString()+@";
                DECLARE @CutOffdate DATE='"+CutOffDateRef+@"';
                DECLARE @ProductId VARCHAR(250)='"+this.product+ @"';

                DECLARE @Enddate DATE=DATEADD(dd, @FutureDays, @CutOffdate);

                WITH daysRange (n, currentdate, enddate) AS (
	                SELECT 0 AS n, @CutOffdate AS currentdate, @Enddate AS enddate
	                UNION ALL
	                SELECT X.n + 1 AS n, dateadd(dd,1,X.currentdate) AS startdate, X.enddate
			                FROM daysRange AS X
			                WHERE dateadd(dd,1,X.currentdate) < X.enddate
                )
                SELECT  
                  CAST(COALESCE(X.[Value], '0') AS REAL) AS [TotalSales],
                  CAST(Y.currentdate AS DATE) AS [SalesDate],
                  CAST(year(Y.currentdate) AS REAL) As [Year]
                FROM daysRange AS Y
                LEFT JOIN dbo.RAW_Calendar AS Z ON CAST(Z.date AS DATE) =Y.currentdate
                LEFT JOIN dbo.RAW_Train_Eval AS X ON X.dCode = Z.d
                ORDER BY CAST(Y.currentdate AS DATE)
            ";

            

            System.Data.SqlClient.SqlClientFactory newFactory = SqlClientFactory.Instance;
            DatabaseSource dbSource = new DatabaseSource(SqlClientFactory.Instance, this.connectionstring, QuerySTR);

            DatabaseLoader loader = ctx.Data.CreateDatabaseLoader<ModelInput>();
            this.DataExtracted = loader.Load(dbSource);

            return QuerySTR;
        }

        public List<ModelOutputExt> PredictData(int horizonRef)
        {
            ModelOutput mo = new ModelOutput();
            var engine=this.model.CreateTimeSeriesEngine<ModelInput, ModelOutput>(this.ctx);
            return Forecast(this.DataExtracted, horizonRef, engine, ctx);
        }

        static List<ModelOutputExt> Forecast(IDataView testData, int horizon, TimeSeriesPredictionEngine<ModelInput, ModelOutput> forecaster, MLContext mlContext)
        {
            ModelOutput forecast = forecaster.Predict();
            List<ModelOutputExt> moextList = new List<ModelOutputExt>();

            IEnumerable<ModelOutputExt> forecastOutput =
            mlContext.Data.CreateEnumerable<ModelInput>(testData, reuseRowObject: false)
                .Take(horizon)
                .Select((ModelInput sale, int index) =>
                {
                    ModelOutputExt mext = new ModelOutputExt();
                    mext.ForecastedSales = (float)(Math.Max(0, Math.Round(forecast.ForecastedSales[index],0)));
                    mext.LowerBoundSales = Math.Max(0, forecast.LowerBoundSales[index]);
                    mext.UpperBoundSales= forecast.UpperBoundSales[index];
                    mext.SalesDate = sale.SalesDate.ToShortDateString();
                    mext.TotalSales = sale.TotalSales;
                    mext.indexX = index;
                    return mext;
                });

            
            foreach(var prediction in forecastOutput)
            {
                moextList.Add(prediction);
            }
            return moextList;
        }
    }
}

