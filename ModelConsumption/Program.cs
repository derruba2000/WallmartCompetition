using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLib;


namespace ModelConsumption
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("->Starting loading model...");
            
            string product = "HOBBIES_1_278_CA_1_evaluation";
            string connectionstring = "Server=localhost;Database=kaggle_wallmart;Integrated Security=True;";
            string ModelsPath= @"c:\temp\WallMartModels";
            int FutureDays = 20;
            int PreviousDays = 0;
            string CutOffDate = "2016-05-23";

            Console.WriteLine("1)----> Creating Predict Object...");
            ModelPredict mlPredict = new ModelPredict(connectionstring, 
                                                      ModelsPath,
                                                      product);
            
            Console.WriteLine("2)----> Extracting data...");
            string QuerySTR=mlPredict.ExtractData(PreviousDays, FutureDays, CutOffDate);
            Console.WriteLine(QuerySTR);

            Console.WriteLine("3)----> Make Predictions...");
            List<ModelLib.ModelOutputExt> predictions =  mlPredict.PredictData(FutureDays);


            foreach (var pred in predictions)
            {
                Console.WriteLine($"---->Date:{pred.SalesDate} " +
                                   $"Actual:{pred.TotalSales} " +
                                   $"Predicted:{pred.ForecastedSales} " +
                                   $"UBound:{pred.UpperBoundSales} " +
                                   $"LBound:{pred.LowerBoundSales} " 
                                   );
            }


        }
    }
}
