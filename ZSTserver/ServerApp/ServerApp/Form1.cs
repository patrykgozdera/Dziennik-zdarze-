using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Configuration;

namespace ServerApp
{
    public partial class Server : Form
    {        
        SqlConnection conn = new SqlConnection(ConfigurationManager.AppSettings["cs"]);    // Connection string z pliku konfiguracyjnego
        string data;        

        public Server()
        {
            InitializeComponent();
        }

        private string ReturnTableList (string str)
        {
            try
            {
                str = String.Empty;
                conn.Open();
                SqlCommand com = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES", conn);
                SqlDataReader reader = com.ExecuteReader();
                while (reader.Read())
                {
                    str += (string)reader["TABLE_NAME"] + ", ";
                }
                conn.Close();
                return str;
            }
            catch (Exception b)
            {
                Communications.Invoke(new Action(delegate ()
                {
                    Communications.Items.Add(b);
                }));
                throw;
            }          
        }        

        public void HttpListenerr(string prefixes)          // źródło: https://msdn.microsoft.com/pl-pl/library/system.net.httplistener(v=vs.110).aspx  
        {
            try
            {
                if (!HttpListener.IsSupported)
                {
                    Communications.Invoke(new Action(delegate ()
                    {
                        Communications.Items.Add("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                    }));
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
                string responseString = String.Empty; //response.StatusCode + " " + response.StatusDescription + Environment.NewLine + "New log created successfully!";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(ReturnTableList(responseString) + Environment.NewLine);

                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                Communications.Invoke(new Action(delegate ()
                {
                    Communications.Items.Add(response.StatusCode + " " + response.StatusDescription);
                    Communications.Items.Add(Environment.NewLine);
                }));

                //close the output stream.
                output.Close();
                listener.Stop();
            }
            catch (Exception b)
            {
                Communications.Invoke(new Action(delegate ()
                {
                    Communications.Items.Add(b);
                }));
            }            
        }

        public void ShowRequestData(HttpListenerRequest request)        // źródło: https://msdn.microsoft.com/en-us/library/system.net.httplistenerrequest.contenttype(v=vs.110).aspx
        {
            try
            {
                if (!request.HasEntityBody)
                {
                    Communications.Invoke(new Action(delegate ()
                    {
                        Communications.Items.Add("No client data was sent with the request.");
                    }));
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
                string string_from_client = reader.ReadToEnd();
                Communications.Invoke(new Action(delegate ()
                {
                    Communications.Items.Add(string_from_client);
                }));
                data = string_from_client;
                Communications.Invoke(new Action(delegate ()
                {
                    Communications.Items.Add("End of client data.");
                }));

                string s;
                char[] chars = data.ToCharArray();

                if (chars[0] == '1')
                {
                    s = "create table ";
                    for (int j = 1; j < chars.Length - 1; j++)
                    {
                        char c = chars[j];
                        if (c != '&')
                        {
                            s += c;
                        }
                        if (c == '&')
                        {
                            for (int i = j + 1; i < chars.Length - 1; i++)
                            {
                                c = chars[i];
                                if (c == '#')
                                {
                                    s += ",";
                                    j++;
                                }

                                else
                                {
                                    s += c;
                                    j++;
                                }
                            }
                        }
                    }

                    s += ");";
                    MessageBox.Show(s);
                    SqlCommand cmd = new SqlCommand(s, conn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Table created successfully!");
                    conn.Close();
                }

                else if (chars[0] == '2')
                {
                    s = "insert into ";
                    for (int j = 1; j < chars.Length - 1; j++)
                    {
                        char c = chars[j];
                        if (c != '&')
                        {
                            s += c;
                        }
                        if (c == '&')
                        {
                            for (int i = j + 1; i < chars.Length - 1; i++)
                            {
                                c = chars[i];
                                if (c == '#')
                                {
                                    s += ",";
                                    j++;
                                }

                                else
                                {
                                    s += c;
                                    j++;
                                }
                            }
                        }
                    }

                    s += ");";
                    MessageBox.Show(s);
                    SqlCommand cmd = new SqlCommand(s, conn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Table filled successfully!");
                    conn.Close();
                }

                body.Close();
                reader.Close();
            }
            catch (Exception b)
            {
                MessageBox.Show(b.Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            try
            {
                MessageBox.Show("Server has stopped waiting for incoming connections!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
                conn.Close();
                this.Dispose(true);
            }
            catch (Exception b)
            {
                Communications.Invoke(new Action(delegate ()
                {
                    Communications.Items.Add(b);
                }));
            }           
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)     //obsługa w innym wątku 
        {                                  
            try
            {                
                while (true)
                {
                        HttpListenerr("http://" + textBox6.Text + ":" +textBox1.Text + "/");                 
                }

            }
            catch (IOException)
            {
                Communications.Invoke(new Action(delegate ()
                {
                    Communications.Items.Add("Problem with client communication.");
                }));
            }       
            
        }        

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Processing cancelled", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information); // Communications.Items.Add("Processing cancelled");
            }
            else if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information); //Communications.Items.Add(e.Error.Message);
            }
            else
            {
                MessageBox.Show(e.Result.ToString(), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information); //Communications.Items.Add(e.Result.ToString());
            }
        }


        private void button3_Click(object sender, EventArgs e)      //ładowanie widoku tabeli 
        {            
            try
            {
                conn.Open();
                SqlDataAdapter sda = new SqlDataAdapter("select * from " + textBox2.Text, conn);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                dataGridView1.DataSource = dt;
                sda.Update(dt);                
                conn.Close();
            }
            catch (Exception b)
            {
                MessageBox.Show(b.Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                conn.Close();
            }            
        }

        private void button5_Click(object sender, EventArgs e)      //filtrowanie
        {            
            try
            {
                conn.Open();
                SqlDataAdapter sda = new SqlDataAdapter("select * from " + textBox2.Text + " where " + textBox3.Text + textBox5.Text + "'"+textBox4.Text+"'", conn);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                dataGridView1.DataSource = dt;
                conn.Close();
            }

            catch (Exception)
            {
                MessageBox.Show("Type values to filter!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                conn.Close();
            }            
        }

        private void button6_Click(object sender, EventArgs e)      //usuwanie 
        {
            SqlCommand cmd = new SqlCommand("delete " + textBox2.Text + " where " + textBox3.Text + textBox5.Text + "'" + textBox4.Text + "'", conn);
            
            try
            {
                conn.Open();
                cmd.Parameters.AddWithValue(textBox3.Text, textBox4.Text);
                cmd.ExecuteNonQuery();                
                MessageBox.Show("Record deleted successfully!");
                SqlDataAdapter sda = new SqlDataAdapter("select * from " + textBox2.Text, conn);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                dataGridView1.DataSource = dt;
                conn.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Type values to delete!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                conn.Close();
            }            
        }
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                Communications.Invoke(new Action(delegate ()
                {
                    string table_list = String.Empty;
                    Communications.Items.Add("Defined types of events: " + ReturnTableList(table_list));                    
                    Communications.Items.Add(Environment.NewLine);
                }));
            }
            catch (Exception)
            {
                MessageBox.Show("Type values to delete!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }            
        }
    }
}
