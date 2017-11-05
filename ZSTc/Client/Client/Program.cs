using System;
using System.Net.Http;
using System.Threading;
using System.Configuration;

namespace Client
{
    class ClientHandler
    {
        string uri = ConfigurationManager.AppSettings["uri"];      // URI z pliku konfiguracyjnego 
        public ClientHandler()
        {
           
        }

        public void ShowMenu()
        {            
            Console.WriteLine("1: Define new type of event.");
            Console.WriteLine("2: Send new events to the server.");
            Console.WriteLine("3: Get a list of all saved types of events.");
            Console.WriteLine("4: Get a list of all avaliable data types.");
            Console.WriteLine("5: Close the application.");
        }
        public async void PostData(string responseString)
        {
            try
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                var byteCont = new ByteArrayContent(buffer);

                var myHttpClient = new HttpClient();
                var response = await myHttpClient.PostAsync(uri, byteCont);
                myHttpClient.Dispose();
            }
            catch (Exception b)
            {
                Console.WriteLine(b);
            }         
        }

        public async void GetData()
        {
            try
            {
                var httpClient = new HttpClient();
                var resp = await httpClient.GetAsync(uri);
                var stringContent = await resp.Content.ReadAsStringAsync();
                Console.WriteLine(stringContent);
                httpClient.Dispose();
            }
            catch (Exception b)
            {
                Console.WriteLine(b);
            }
            
        }

        
    }
}
