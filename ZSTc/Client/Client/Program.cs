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
        string uri = "http://localhost:30000/";
        public ClientHandler()
        {
           
        }

        private void ShowMenu()
        {
            Console.WriteLine("Welcome to the application!" + Environment.NewLine);
            Console.WriteLine("1: Define new type of event.");
            Console.WriteLine("2: Send new events to the server.");
            Console.WriteLine("3: Close the application.");
        }
        private async void PostData(string responseString)
        {                 
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
            string s = String.Empty;
            string type = String.Empty;
            string rs = String.Empty;
            string resString;
            ClientHandler ch = new ClientHandler();

            try
            {
                ch.ShowMenu();
                
                bool isRunning = true;

                while (isRunning)
                {
                    int caseswitch = Int32.Parse(Console.ReadLine());

                    switch (caseswitch)
                    {
                        case 1:
                            Console.WriteLine("Enter a type of new event.");
                            type = Console.ReadLine();
                            Console.WriteLine("First, name all columns with their types in a event's log ('Esc' finishes this operation):");
                            while (true)
                            {
                                Console.WriteLine("Now enter name and type of column, separated by a space bar (for example: ID int)");
                                s = Console.ReadLine();
                                if (s.Equals("Esc"))
                                {
                                    break;
                                }
                                rs += s;
                                rs += "#";
                            }
                            resString = "1" + type + "(&Event_time varchar(255)#" + rs;                            
                            ch.PostData(resString);
                            rs = String.Empty;
                            ch.ShowMenu();
                            break;

                        case 2:
                            DateTime dt = DateTime.Now;
                            Console.WriteLine("First, choose type of event to fill.");
                            type = Console.ReadLine();
                            Console.WriteLine("Now create new event by giving values of each column in log table");
                            while (true)
                            {
                                s = Console.ReadLine();
                                if (s.Equals("Esc"))
                                {
                                    break;
                                }
                                rs += "'" + s + "'";
                                rs += "#";
                            }
                            resString = "2" + type + " values (&" + "'" + dt.ToString("t") + "'" + "," + rs;                           
                            ch.PostData(resString);
                            rs = String.Empty;
                            ch.ShowMenu();
                            break;

                        case 3:
                            Environment.Exit(0);
                            break;

                        default:
                            isRunning = false;
                            break;
                    }
                }
                                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }            
                        
        } 
    }
}
