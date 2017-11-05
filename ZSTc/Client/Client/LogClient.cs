using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class LogClient
    {
        public LogClient( ClientHandler ch)
        {
            
        }
        public static void Main()
        {
            string s = String.Empty;
            string type = String.Empty;
            string rs = String.Empty;
            string resString;
            var guid = Guid.NewGuid().ToString();
            ClientHandler ch = new ClientHandler();

            try
            {
                Console.WriteLine("Welcome to the application!" + Environment.NewLine);
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
                            resString = "1" + type + "(&ID varchar(MAX)#Event_time varchar(255)#" + rs;
                            ch.PostData(resString);
                            rs = String.Empty;
                            Console.Clear();
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
                            resString = "2" + type + " values (&" + "'" + guid + "'" + ",'" + dt.ToString("d MMM yyyy, HH:mm:ss.ff") + "'," + rs;
                            ch.PostData(resString);
                            rs = String.Empty;
                            Console.Clear();
                            ch.ShowMenu();
                            break;

                        case 3:
                            ch.GetData();
                            Thread.Sleep(700);
                            ch.ShowMenu();
                            break;

                        case 4:
                            Console.WriteLine("bigint, binary(50), bit, char(10), date, datetime, datetime2(7)," + Environment.NewLine+
                                "datetimeoffset(7), decimal(18,0), float, hierarchyid, image, int, " + Environment.NewLine +
                                "money, nchar(10), ntext, numeric(18,0), nvarchar(50), nvarchar(max), " + Environment.NewLine +
                                "real, smalldatetime, smallint, smallmoney, sql_variant, table, text, " + Environment.NewLine +
                                "time(7), timestamp, tinyint, uniqueidentifier, varbinary(50), " + Environment.NewLine +
                                "varbinary(max), varchar(50), varchar(max), xml" + Environment.NewLine);
                            ch.ShowMenu();
                            break;
                        case 5:
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
