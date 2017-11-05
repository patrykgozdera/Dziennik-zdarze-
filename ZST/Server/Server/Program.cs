using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = "Server=.\\SQLExpress;Database=BazaZST;Integrated Security=true";
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT id_zdarzenia, typ, czas FROM [dbo].[Table_1]", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("{0} {1} {2}", reader.GetInt32(0), reader.GetString(1), reader.GetDateTime(2));
                }
                reader.Close();
                conn.Close();

                if (Debugger.IsAttached)
                {
                    Console.ReadLine();
                }
            }
        }
    }
}
