using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace ModelTrainer
{
    class dbChecks
    {
        public void ExecuteQuery(string connectionString, string Query)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(Query, connection);
            connection.Open();
            Console.WriteLine("Executing query...");
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {

                    var TotalSales = reader.GetValue(0);
                    var SalesDate = reader.GetValue(1);
                    var tmpYear = reader.GetValue(2);

                    Console.WriteLine($"--->{TotalSales.ToString()},{SalesDate.ToString()},{tmpYear.ToString()}");
                }
            }
            connection.Close();

        }
    }
}
