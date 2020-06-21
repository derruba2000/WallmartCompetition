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
        IDataView dataView;

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

        public void ExtractData(int PreviousDaysRef, int FutureDaysRef, string CutOffDateRef)
        {
            string QuerySTR = @"
                DECLARE @FutureDays INT  = "+FutureDaysRef.ToString()+@";
                DECLARE @PreviousDays INT  = "+PreviousDaysRef.ToString()+@";
                DECLARE @CutOffdate DATE='"+CutOffDateRef+@"';
                DECLARE @ProductId VARCHAR(250)='"+this.product+@"';

                 SELECT 
                    CAST(X.[Value] AS REAL) AS [TotalSales],
                    CAST(Y.date AS DATE) AS [SalesDate],
	                CAST(year(Y.date) AS REAL) As [Year]
                FROM [dbo].[RAW_Train_Eval] AS X
                INNER JOIN [dbo].RAW_Calendar AS Y ON Y.d=X.dCode
                WHERE Id=@ProductId
                AND CAST(Y.date AS DATE) BETWEEN DATEADD(dd, -@PreviousDays , @CutOffdate) AND DATEADD(dd, @FutureDays, @CutOffdate)

            ";

            System.Data.SqlClient.SqlClientFactory newFactory = SqlClientFactory.Instance;
            DatabaseSource dbSource = new DatabaseSource(SqlClientFactory.Instance, this.connectionstring, QuerySTR);

            DatabaseLoader loader = ctx.Data.CreateDatabaseLoader<ModelInput>();
            this.dataView = loader.Load(dbSource);


        }

        public  List<ModelOutputExt> PredictData(int horizonRef)
        {
            
            ModelOutput mo = new ModelOutput();
            var engine=this.model.CreateTimeSeriesEngine<ModelInput, ModelOutput>(this.ctx);

            ModelOutput forecast = engine.Predict();

            IEnumerable<ModelOutputExt> forecastOutput =
            ctx.Data.CreateEnumerable<ModelInput>(this.DataExtracted, reuseRowObject: false)
                .Take(horizonRef)
                .Select((ModelInput sale, int index) =>
                {
                    ModelOutputExt mext = new ModelOutputExt();
                    mext.ForecastedSales = forecast.ForecastedSales[index];
                    mext.LowerBoundSales = Math.Max(0, forecast.LowerBoundSales[index]);
                    mext.UpperBoundSales = forecast.UpperBoundSales[index];
                    mext.SalesDate = sale.SalesDate.ToShortDateString();
                    mext.TotalSales = sale.TotalSales;
                    return mext;
                });

            return forecastOutput;

        }

        static IEnumerable<ModelOutputExt> Forecast(IDataView testData, int horizon, TimeSeriesPredictionEngine<ModelInput, ModelOutput> forecaster, MLContext mlContext)
        {
            ModelOutput forecast = forecaster.Predict();

            IEnumerable<ModelOutputExt> forecastOutput =
            mlContext.Data.CreateEnumerable<ModelInput>(testData, reuseRowObject: false)
                .Take(horizon)
                .Select((ModelInput sale, int index) =>
                {
                    ModelOutputExt mext = new ModelOutputExt();
                    mext.ForecastedSales = forecast.ForecastedSales[index];
                    mext.LowerBoundSales = Math.Max(0, forecast.LowerBoundSales[index]);
                    mext.UpperBoundSales= forecast.UpperBoundSales[index];
                    mext.SalesDate = sale.SalesDate.ToShortDateString();
                    mext.TotalSales = sale.TotalSales;
                    return mext;
                });

            return forecastOutput;

            

        }




    }
}

