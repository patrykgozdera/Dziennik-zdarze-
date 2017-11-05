using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Serwer
{
    class Program
    {
        private static void LoadFromDB()
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

        private static void ProcessClientRequests(object argument)
        {
            TcpClient client = (TcpClient)argument;
            try
            {
                StreamReader reader = new StreamReader(client.GetStream());
                StreamWriter writer = new StreamWriter(client.GetStream());
                string s = String.Empty;
                while (!(s = reader.ReadLine()).Equals("Exit") || (s == null))
                {
                    Console.WriteLine("From client -> " + s);
                    writer.WriteLine("From server -> " + s);
                    writer.Flush();
                }
                reader.Close();
                writer.Close();
                client.Close();
                Console.WriteLine("Closing client connection!");
            }
            catch (IOException)
            {
                Console.WriteLine("Problem with client communication. Exiting thread.");
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }
            }
        }

        public static void Main()
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
                listener.Start();
                Console.WriteLine("MultiThreadedEchoServer started...");
                Console.WriteLine("Type 'LOAD' to load date from the DB.");
                while (true)
                {
                    if (Console.ReadLine() == "LOAD")
                    {
                        LoadFromDB();
                    }
                    Console.WriteLine("Waiting for incoming client connections...");
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Accepted new client connection...");
                    Thread t = new Thread(ProcessClientRequests);
                    t.Start(client);
                    
                }
               
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                if (listener != null)
                {
                    listener.Stop();
                }
            }
        }
    }
}
