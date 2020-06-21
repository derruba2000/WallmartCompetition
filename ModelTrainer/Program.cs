using System;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ModelTrainer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Training time series analysis");
            //Step 1. Create a ML Context
            var ctx = new MLContext();

            string connectionString = "Data Source=localhost;Initial Catalog=kaggle_wallmart;Provider=SQLNCLI11.1;Integrated Security=SSPI;Auto Translate=False;";
            connectionString = "Server=localhost;Database=kaggle_wallmart;Integrated Security=True";

            string Query = @"
                SELECT 
                      CAST(X.[Value] AS REAL) AS [TotalSales],
                      CAST(Y.date AS DATE) AS [SalesDate],
	                  CAST(year(Y.date) AS REAL) As [Year]
                  FROM [dbo].[RAW_Train_Eval] AS X
                  INNER JOIN [dbo].RAW_Calendar AS Y ON Y.d=X.dCode
                  where Id='HOBBIES_1_278_CA_1_evaluation' 
                  order by 2

            ";

            Console.WriteLine("Connecting to the database...");
            //dbChecks dbchecks = new dbChecks();
            //dbchecks.ExecuteQuery(connectionString, Query);

            
            System.Data.SqlClient.SqlClientFactory newFactory = SqlClientFactory.Instance;
            Console.WriteLine("Loading data...");
            DatabaseSource dbSource = new DatabaseSource(SqlClientFactory.Instance, connectionString, Query);
            DatabaseLoader loader = ctx.Data.CreateDatabaseLoader<ModelInput>();
            IDataView dataView = loader.Load(dbSource);
            Console.WriteLine($"Loaded {dataView.GetRowCount()} rows...");

            IDataView trainingData = ctx.Data.FilterRowsByColumn(dataView, "Year", upperBound: 2016);
            IDataView ValidationData = ctx.Data.FilterRowsByColumn(dataView, "Year", lowerBound: 2016);

            var forecastingPipeline = ctx.Forecasting.ForecastBySsa(
                    outputColumnName: "ForecastedSales",
                    inputColumnName: "TotalSales",
                    windowSize: 7,
                    seriesLength: 30,
                    trainSize: 1798,
                    horizon: 30,
                    confidenceLevel: 0.95f,
                    confidenceLowerBoundColumn: "LowerBoundSales",
                    confidenceUpperBoundColumn: "UpperBoundSales");

            SsaForecastingTransformer forecaster = forecastingPipeline.Fit(trainingData);

            Evaluate(ValidationData, forecaster, ctx);

            var forecastEngine = forecaster.CreateTimeSeriesEngine<ModelInput, ModelOutput>(ctx);
            forecastEngine.CheckPoint(ctx, "c:\\temp\\Model.zip");


            Forecast(ValidationData, 7, forecastEngine, ctx);


            Console.WriteLine("Training time series analysis completed");

        }

        static void Evaluate(IDataView testData, ITransformer model, MLContext mlContext)
        {
            IDataView predictions = model.Transform(testData);

            IEnumerable<float> actual =
                    mlContext.Data.CreateEnumerable<ModelInput>(testData, true)
                    .Select(observed => observed.TotalSales);

            IEnumerable<float> forecast =  mlContext.Data.CreateEnumerable<ModelOutput>(predictions, true)
                .Select(prediction => prediction.ForecastedSales[0]);




            var metrics = actual.Zip(forecast, (actualValue, forecastValue) => actualValue - forecastValue);

            var MAE = metrics.Average(error => Math.Abs(error)); // Mean Absolute Error
            var RMSE = Math.Sqrt(metrics.Average(error => Math.Pow(error, 2))); // Root Mean Squared Error

            Console.WriteLine("Evaluation Metrics");
            Console.WriteLine("---------------------");
            Console.WriteLine($"Mean Absolute Error: {MAE:F3}");
            Console.WriteLine($"Root Mean Squared Error: {RMSE:F3}\n");

            Console.WriteLine("Rental Forecast");
            Console.WriteLine("---------------------");

            

        }

        static void Forecast(IDataView testData, int horizon, TimeSeriesPredictionEngine<ModelInput, ModelOutput> forecaster, MLContext mlContext)
        {
            ModelOutput forecast = forecaster.Predict();

            IEnumerable<string> forecastOutput =
            mlContext.Data.CreateEnumerable<ModelInput>(testData, reuseRowObject: false)
                .Take(horizon)
                .Select((ModelInput sale, int index) =>
                {
                    string SaleDate = sale.SalesDate.ToShortDateString();
                    float actualSale = sale.TotalSales;
                    float lowerEstimate = Math.Max(0, forecast.LowerBoundSales[index]);
                    float estimate = forecast.ForecastedSales[index];
                    float upperEstimate = forecast.UpperBoundSales[index];
                    return $"Date: {SaleDate}\n" +
                    $"Actual Rentals: {actualSale}\n" +
                    $"Lower Estimate: {lowerEstimate}\n" +
                    $"Forecast: {estimate}\n" +
                    $"Upper Estimate: {upperEstimate}\n";
                });

            foreach (var prediction in forecastOutput)
            {
                Console.WriteLine(prediction);
            }

        }


    }
}
