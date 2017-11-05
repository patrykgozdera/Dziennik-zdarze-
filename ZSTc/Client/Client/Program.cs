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
            string s = String.Empty;
            string rs = String.Empty;
            Console.WriteLine("Hello, type a new event. 'Esc' finishes this operation.");
            while (true)
            {                
                s = Console.ReadLine();                
                if (s.Equals("Esc"))
                {
                    break;
                }
                rs += s;
                rs += "#";
            }
            DateTime dt = DateTime.Now;
            string responseString = rs;//"Event time#" + dt.ToString("t") + rs;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            var byteCont = new ByteArrayContent(buffer);

            var myHttpClient = new HttpClient();
            var response = await myHttpClient.PostAsync(uri, byteCont);
            var stringContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(stringContent);
            myHttpClient.Dispose();
            }
        public static void Main()
        {
            try
            {
                ClientHandler ch = new ClientHandler();                   
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.ReadKey();
                        
        } 
    }
}
