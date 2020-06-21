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
            string connectionstring = "Data Source=localhost;Initial Catalog=kaggle_wallmart;Provider=SQLNCLI11.1;Integrated Security=SSPI;Auto Translate=False;";
            string ModelsPath= @"c:\temp\WallMartModels";
            int FutureDays = 7;
            int PreviousDays = 60;
            string CutOffDate = "2016-03-12";

            ModelPredict mlPredict = new ModelPredict(connectionstring, 
                                                      ModelsPath,
                                                      product);
            
        }
    }
}
