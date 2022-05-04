using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Client : Form
    {
        public Client()
        {
            InitializeComponent();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void Client_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            label1.Text = "本机IP：" + GetMyIpAddress();
            textBoxSerIP.Text = GetMyIpAddress();
            listBox1.Items.Clear();
            listBox1.Items.Add(GetMyIpAddress() + ":" + SERVER_LISTEN_PORT);
        }

        public ushort localSendPort = 19132;
        public static TcpClient tcpClient;
        private Listener listener;
        public static ushort SERVER_LISTEN_PORT = 5656;
        private Sender sen;
        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            numericUpDown1.ReadOnly = true;
            numericUpDown2.ReadOnly = true;
            textBoxSerIP.ReadOnly = true;
            SERVER_LISTEN_PORT = (ushort)numericUpDown2.Value;
            localSendPort = (ushort)numericUpDown1.Value;
            string msg = textBoxMsg.Text;
            Thread threadConnect = new Thread(ConnectoServer);
            threadConnect.IsBackground = true;
            threadConnect.Start();
        }
        private delegate void  RefreshListBoxDelegate(string t);
        private delegate void CleanListBoxDelegate();
        public void refreshListBox(string txt)
        {
            string[] lines = txt.Split('\n');
            if (listBox1.InvokeRequired)
            {
                RefreshListBoxDelegate rld = delegate (string t) { 
                    listBox1.Items.Add(t);
                    listBox1.Refresh();
                };
                CleanListBoxDelegate cld = () => { listBox1.Items.Clear(); };
                listBox1.Invoke(cld);
                listBox1.Invoke(rld, GetMyIpAddress() + ":" + SERVER_LISTEN_PORT);
                for (int i=0;i<lines.Length;i++)
                {
                    string line = lines[i];
                    listBox1.Invoke(rld, line);
                }
            }
            else
            {
                listBox1.Items.Add(GetMyIpAddress() + ":" + SERVER_LISTEN_PORT);
                listBox1.Items.Clear();
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    listBox1.Items.Add(line);
                }
            }
        }

        private void ConnectoServer()
        {
            AsyncCallback requestcallback;
            try
            {
                requestcallback = new AsyncCallback(RequestCallBack);
                tcpClient = new TcpClient(AddressFamily.InterNetwork);
                IPAddress addr = new IPAddress(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].Address);
                tcpClient.Client.Bind(new IPEndPoint(addr, localSendPort));
            }
            catch (SocketException ex)
            {
                toolStripStatusLabel1.Text = "连接失败，原因：端口号被占用，请更换端口重试";
                numericUpDown1.Value += 1;
                button2.Enabled = true;
                numericUpDown1.ReadOnly = !true;
                numericUpDown2.ReadOnly = !true;
                textBoxSerIP.ReadOnly = !true;
                return;
            }catch(Exception ex)
            {
                MessageBox.Show("error: " + ex.Message);
                button2.Enabled = true;
                numericUpDown1.ReadOnly = !true;
                numericUpDown2.ReadOnly = !true;
                textBoxSerIP.ReadOnly = !true;
                return;
            }
            tcpClient.Client.IOControl(IOControlCode.KeepAliveValues, GetKeepAliveData(), null);
            tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            IAsyncResult result = tcpClient.BeginConnect(IPAddress.Parse(textBoxSerIP.Text), SERVER_LISTEN_PORT, requestcallback, tcpClient);
        }

        private void RequestCallBack(IAsyncResult iar)
        {
            try
            {
                tcpClient = (TcpClient)iar.AsyncState;
                if (tcpClient != null)
                { 
                    start_listen();
                    NetworkStream tcpStream = tcpClient.GetStream();
                    string msg = Sender.addHeader("上线了"+Environment.NewLine, Sender.CONNECT, "");
                    Byte[] data = Encoding.UTF8.GetBytes(msg);
                    tcpStream.Write(data, 0, data.Length);
                    toolStripStatusLabel1.Text = "当前状态：已连接到服务器";
                }
            }
            catch (Exception ex)
            {
                button2.Enabled = true;
                numericUpDown1.ReadOnly = !true;
                numericUpDown2.ReadOnly = !true;
                textBoxSerIP.ReadOnly = !true;
                toolStripStatusLabel1.Text = "当前状态：连接失败 "+ex.Message;
            }
        }

        public void setStatus(string msg)
        {
            toolStripStatusLabel1.Text = msg;
        }

        private void start_listen()
        {
            try
            {
                if (listener.listenerRun == true)
                {
                    listener.listenerRun = false;
                    listener.Stop();
                }
            }
            catch (NullReferenceException)
            {
            }
            finally
            {
                listener = new Listener((ushort)(localSendPort + 10000));
                listener.OnAddMessage += new EventHandler<AddMessageEventArgs>(this.AddMessage);
                listener.StartListener();
            }
        }

        public void AddMessage(object sender, AddMessageEventArgs e)
        {
            string message = e.mess;
            string appendText;
            string[] sep = message.Split('>');
            appendText = sep[0] + ">:           " + System.DateTime.Now.ToString() + Environment.NewLine + sep[1] + Environment.NewLine;
            int txtGetMsgLength = this.richTextBox1.Text.Length;
            this.richTextBox1.AppendText(appendText);
            this.richTextBox1.Select(txtGetMsgLength, appendText.Length - Environment.NewLine.Length * 2 - sep[1].Length);
            this.richTextBox1.SelectionColor = Color.Red;
            this.richTextBox1.ScrollToCaret();
        }

        private static string GetMyIpAddress()
        {
            IPAddress addr = new System.Net.IPAddress(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].Address);
            return addr.ToString();
        }

        private byte[] GetKeepAliveData()
        {
            uint dummy = 0;
            byte[] inOptionValues = new byte[4 * 3];
            BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)3000).CopyTo(inOptionValues, 4);//keep-alive间隔
            BitConverter.GetBytes((uint)500).CopyTo(inOptionValues, 4 * 2);// 尝试间隔
            return inOptionValues;
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            try
            {
                button2.Enabled = !false;
                numericUpDown1.ReadOnly = !true;
                numericUpDown2.ReadOnly = !true;
                textBoxSerIP.ReadOnly = !true;
                NetworkStream tcpStream = tcpClient.GetStream();
                string msg = Sender.addHeader("", Sender.DISCONNECT, "");
                Byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(msg);
                tcpStream.Write(data, 0, data.Length);
                listener.Stop();
                tcpStream.Close();
                tcpClient.Close();
                listener = null;
                tcpClient = null;
                toolStripStatusLabel1.Text = "连接已断开";
            }catch(NullReferenceException ex)
            {
                toolStripStatusLabel1.Text = ex.Message;
            }catch(Exception ex)
            {
                toolStripStatusLabel1.Text = ex.Message;
            }
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            if (textBoxSerIP.Text.Trim() == "")
            {
                MessageBox.Show("请选择目标主机");
                return;
            }
            else if (textBoxMsg.Text.Trim() == "")
            {
                MessageBox.Show("消息内容不能为空!", "错误");
                this.textBoxMsg.Focus();
                return;
            }
            else
            {
                try
                {
                    sen = new Sender(textBoxSerIP.Text);
                    string msg;
                    if (textBoxTrans.Text==""||textBoxTrans.Text==textBoxSerIP.Text||textBoxTrans.Text==textBoxSerIP.Text+":"+SERVER_LISTEN_PORT)
                    {
                        msg = Sender.addHeader(textBoxMsg.Text, Sender.DIRECT, "");
                    }
                    else
                    {
                        msg = Sender.addHeader(textBoxMsg.Text, Sender.TRANSFER, textBoxTrans.Text);
                    }
                    sen.Send(msg);
                    string appendText;
                    appendText = "Me:   " + System.DateTime.Now.ToString() + Environment.NewLine + textBoxMsg.Text + Environment.NewLine;
                    int txtGetMsgLength = this.richTextBox1.Text.Length;
                    this.richTextBox1.AppendText(appendText);
                    this.richTextBox1.Select(txtGetMsgLength, appendText.Length - Environment.NewLine.Length * 2 - textBoxSerIP.Text.Length);
                    this.richTextBox1.SelectionColor = Color.Green;
                    this.richTextBox1.ScrollToCaret();
                }
                catch (Exception ex)
                {
                    toolStripStatusLabel1.Text=ex.Message;
                }
                this.textBoxMsg.Text = "";
                this.textBoxMsg.Focus();
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            sen = new Sender(textBoxSerIP.Text);
            sen.Send(Sender.addHeader("", Sender.REFRESH, ""));
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem  != null)
            {
                textBoxTrans.Text= listBox1.SelectedItem.ToString();
            }
            
        }
    }
}
