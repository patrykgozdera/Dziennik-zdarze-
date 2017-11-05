using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void ProcessClientRequests(object argument)
        {
            //Form1 frmTemp = (Form1)frm;
            TcpClient client = (TcpClient)argument;
            try
            {
                StreamReader reader = new StreamReader(client.GetStream());
                StreamWriter writer = new StreamWriter(client.GetStream());
                string s = String.Empty;
                while (!(s = reader.ReadLine()).Equals("Exit") || (s == null))
                {
                    Communications.Invoke(new Action(delegate ()
                    {
                        Communications.Items.Add("From client -> " + s);
                    }));
                    //Communications.Items.Add("From client -> " + s);
                    //Console.WriteLine("From client -> " + s);
                    writer.WriteLine("From server -> " + s);
                    writer.Flush();
                }
                reader.Close();
                writer.Close();
                client.Close();
                Communications.Invoke(new Action(delegate ()
                {
                    Communications.Items.Add("Closing client connection!");
                }));
                //Communications.Items.Add("Closing client connection!");
                //Console.WriteLine("Closing client connection!");
            }
            catch (IOException)
            {
                //Communications.Invoke(new Action(delegate ()
                //{
                //    Communications.Items.Add("Problem with client communication. Exiting thread.");
                //}));
                //Communications.Items.Add("Problem with client communication. Exiting thread.");
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

        private void button1_Click(object sender, EventArgs e)
        {
            // Check if the backgroundWorker is already busy running the asynchronous operation
            if (!backgroundWorker1.IsBusy)
            {
                // This method will start the execution asynchronously in the background
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
                listener.Start();
                Communications.Invoke(new Action(delegate ()
                {
                    Communications.Items.Add("MultiThreadedEchoServer started...");
                }));
                //Communications.Items.Add("MultiThreadedEchoServer started...");
                while (true)
                {
                    Communications.Invoke(new Action(delegate ()
                    {
                        Communications.Items.Add("Waiting for incoming client connections...");
                    }));
                    //Communications.Items.Add("Waiting for incoming client connections...");
                    TcpClient client = listener.AcceptTcpClient();
                    Communications.Invoke(new Action(delegate ()
                    {
                        Communications.Items.Add("Accepted new client connection...");
                    }));
                    //Communications.Items.Add("Accepted new client connection...");
                    Thread t = new Thread(ProcessClientRequests);
                    t.Start(client);

                }

            }
            catch (IOException)
            {
                Communications.Invoke(new Action(delegate ()
                {
                    Communications.Items.Add("Problem with client communication.");
                }));
                //Communications.Items.Add("Problem with client communication.");
            }
            finally
            {
                if (listener != null)
                {
                    listener.Stop();
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Communications.Items.Add("Processing cancelled");
            }
            else if (e.Error != null)
            {
                Communications.Items.Add(e.Error.Message);
            }
            else
            {
                Communications.Items.Add(e.Result.ToString());
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = "Server=.\\SQLExpress;Database=BazaZST;Integrated Security=true";
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT id_zdarzenia, typ, czas FROM [dbo].[Table_1]", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {   
                    Communications.Invoke(new Action(delegate ()
                    {
                        Communications.Items.Add("{0} {1} {2}", reader.GetInt32(0), reader.GetString(1), reader.GetDateTime(2));
                    }));
                    //Console.WriteLine("{0} {1} {2}", reader.GetInt32(0), reader.GetString(1), reader.GetDateTime(2));
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
