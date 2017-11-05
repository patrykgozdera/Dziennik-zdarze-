﻿using System;
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
    public partial class Server : Form
    {
        SqlConnection conn = new SqlConnection("Server=.\\SQLExpress;Database=BazaZST;Integrated Security=true");

        public Server()
        {
            InitializeComponent();
        }

        public void HttpListenerr(string prefixes)
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            // URI prefixes are required            
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // Create a listener.
            HttpListener listener = new HttpListener();

            // Add the prefixes.
            listener.Prefixes.Add(prefixes);
            listener.Start();
            Communications.Invoke(new Action(delegate ()
            {
                Communications.Items.Add("Listening...");
            }));

            // Note: The GetContext method blocks while waiting for a request. 
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            ShowRequestData(request);

            // Obtain a response object.
            HttpListenerResponse response = context.Response;

            // Construct a response.
            string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            Communications.Invoke(new Action(delegate ()
            {
                Communications.Items.Add(response.StatusCode);
                Communications.Items.Add(Environment.NewLine);
            }));

            // You must close the output stream.
            output.Close();
            listener.Stop();

        }

        public void ShowRequestData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                Console.WriteLine("No client data was sent with the request.");
                return;
            }
            System.IO.Stream body = request.InputStream;
            System.Text.Encoding encoding = request.ContentEncoding;
            System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
            if (request.ContentType != null)
            {
                Communications.Invoke(new Action(delegate ()
                {
                    Communications.Items.Add("Client data content type: " + request.ContentType);
                }));
                
            }
            Communications.Invoke(new Action(delegate ()
            {
                Communications.Items.Add("Client data content length: " + request.ContentLength64);
            }));
            
            Communications.Invoke(new Action(delegate ()
            {
                Communications.Items.Add("Start of client data:");
            }));
            
            // Convert the data to a string and display it on the console.
            string s = reader.ReadToEnd();
            Communications.Invoke(new Action(delegate ()
            {
                Communications.Items.Add(s);
            }));
            
            Communications.Invoke(new Action(delegate ()
            {
            Communications.Items.Add("End of client data.");
            }));
           
            body.Close();
            reader.Close();
        }

        private void ProcessClientRequests(object argument)         //obsługa połączenia
        {
            //Form1 frmTemp = (Form1)frm;
            TcpClient client = (TcpClient)argument;
            try
            {
                StreamReader reader = new StreamReader(client.GetStream());
                StreamWriter writer = new StreamWriter(client.GetStream());
                string s = String.Empty;
                int j = 1;
                while (!(s = reader.ReadLine()).Equals("Exit") || (s == null))
                {
                    Communications.Invoke(new Action(delegate ()
                    {
                        Communications.Items.Add("From client_"+j+" -> " + s);
                    }));
                    writer.WriteLine("From server -> " + s);
                    writer.Flush();
                    j++;
                }
                reader.Close();
                writer.Close();
                client.Close();
                Communications.Invoke(new Action(delegate ()
                {
                    Communications.Items.Add("Closing client connection!");
                }));               
            }
            catch (IOException)
            {
                //Communications.Invoke(new Action(delegate ()
                //{
                //    Communications.Items.Add("Problem with client communication. Exiting thread.");
                //}));
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

        private void button1_Click(object sender, EventArgs e)      //rozpoczęcie nasłuchiwania 
        {            
            if (!backgroundWorker1.IsBusy)
            {                
                backgroundWorker1.RunWorkerAsync();
                            }
        }

        private void button2_Click(object sender, EventArgs e)      //zakończenie pracy serwera
        {
            MessageBox.Show("Server has stopped waiting for incoming connections!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
            conn.Close();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)     //obsługa w innym wątku 
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Parse("10.1.1.15"), int.Parse(textBox1.Text));
                listener.Start();
                Communications.Invoke(new Action(delegate ()
                {
                    Communications.Items.Add("MultiThreadedEchoServer started...");
                }));
                int i = 1;
                while (true)
                {
                        Communications.Invoke(new Action(delegate ()
                        {
                            Communications.Items.Add("Waiting for incoming client connections...");
                        }));

                        TcpClient client = listener.AcceptTcpClient();
                        Communications.Invoke(new Action(delegate ()
                        {
                            Communications.Items.Add("Accepted client_" + i + " connection...");
                        }));

                        Thread t = new Thread(ProcessClientRequests);
                        t.Start(client);
                        HttpListenerr("http://localhost:30000/");
                    i++;
                }

            }
            catch (IOException)
            {
                Communications.Invoke(new Action(delegate ()
                {
                    Communications.Items.Add("Problem with client communication.");
                }));
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
                MessageBox.Show("Processing cancelled"); // Communications.Items.Add("Processing cancelled");
            }
            else if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message); //Communications.Items.Add(e.Error.Message);
            }
            else
            {
                MessageBox.Show(e.Result.ToString()); //Communications.Items.Add(e.Result.ToString());
            }
        }


        private void button3_Click(object sender, EventArgs e)      //ładowanie widoku tabeli 
        {
            conn.Open();
            SqlDataAdapter sda = new SqlDataAdapter("select * from " +textBox2.Text, conn);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            dataGridView1.DataSource = dt;
            sda.Update(dt);
            conn.Close();
        }

        private void button5_Click(object sender, EventArgs e)      //filtrowanie
        {
            try
            {
                conn.Open();
                SqlDataAdapter sda = new SqlDataAdapter("select * from " + textBox2.Text + " where " + textBox3.Text + " = " + "'"+textBox4.Text+"'", conn);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                dataGridView1.DataSource = dt;
                conn.Close();
            }

            catch (IOException)
            {
                MessageBox.Show("Type values to filter!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            SqlCommand cmd = new SqlCommand("delete " + textBox2.Text + " where " + textBox3.Text + " = " + "'"+textBox4.Text+"'", conn);
            conn.Open();
            cmd.Parameters.AddWithValue(textBox3.Text, textBox4.Text);
            cmd.ExecuteNonQuery();
            conn.Close();
            MessageBox.Show("Record Deleted Successfully!");
        }
    }
}
