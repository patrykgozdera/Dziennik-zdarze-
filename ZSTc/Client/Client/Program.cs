using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace Client
{
    class ClientHandler
    {
        public ClientHandler()
        {
            this.PostData();
        }
        private async void PostData()
        {
            string uri = "http://localhost:30000/";
            var comment = "hello world";
            var questionId = "ui";

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("comment", comment),
                new KeyValuePair<string, string>("questionId", questionId)
            });

            var myHttpClient = new HttpClient();
            var response = await myHttpClient.PostAsync(uri, formContent);
            var stringContent = await response.Content.ReadAsStringAsync();
        }
        public static void Main()
        {
            try
            {
                //Console.WriteLine("Podaj IP");
                //string a = Console.ReadLine();
                //Console.WriteLine("Podaj port");
                //string b = Console.ReadLine();
                //int c = int.Parse(b);
                //TcpClient client = new TcpClient(a, c);
                TcpClient client = new TcpClient("10.1.1.15", 8080);
                StreamReader reader = new StreamReader(client.GetStream());
                StreamWriter writer = new StreamWriter(client.GetStream());
                String s = String.Empty;
                while (!s.Equals("Exit"))
                {
                    ClientHandler ch = new ClientHandler();
                    Console.Write("Enter a string to send to the server: ");
                    s = Console.ReadLine();
                    Console.WriteLine();
                    writer.WriteLine(s);
                    writer.Flush();
                    String server_string = reader.ReadLine();
                    Console.WriteLine(server_string);
                                    }
                reader.Close();
                writer.Close();
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.ReadKey();
                        
        } 
    }
}
