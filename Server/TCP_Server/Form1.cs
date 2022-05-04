using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Configuration;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Threading;

namespace TCP_Server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public bool appRun = true;
        private Listener lis;//监听对象
        private Sender sen;//发送对象
        string chatToIp;
        ushort chatToPort;

        //返回信息
        public void AddMessage(object sender, AddMessageEventArgs e)
        {
            string message = e.mess;
            string appendText;
            string[] sep = message.Split('>');
            string[] sepIp = sep[0].Split('<', ':');
            bool checkIp = true;
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                if (listBox1.Items[i].ToString() == sepIp[1]+":"+sepIp[2])
                    checkIp = false;
            }
            if (checkIp && sep[1].Trim() != "断开")
            {
                this.listBox1.Items.Add(sepIp[1].Trim()+":"+ sepIp[2]);
                chatToIp = sepIp[1];
                chatToPort = UInt16.Parse( sepIp[2]);
            }

            appendText = sep[0] + ">:           " + System.DateTime.Now.ToString() + Environment.NewLine + sep[1] + Environment.NewLine;
            int txtGetMsgLength = this.richTextBox1.Text.Length;
            this.richTextBox1.AppendText(appendText);
            this.richTextBox1.Select(txtGetMsgLength, appendText.Length - Environment.NewLine.Length * 2 - sep[1].Length);
            this.richTextBox1.SelectionColor = Color.Red;
            this.richTextBox1.ScrollToCaret();
        }

        //下线
        public void IpRemo(object sender, AddMessageEventArgs e)
        {
            //string[] sep = e.mess.Split(':');
            try
            {
                int index = 0;
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    string t1 = listBox1.Items[i].ToString();
                    string t2 = e.mess;
                    if (listBox1.Items[i].ToString() == e.mess)
                    {
                        index = i;
                        this.listBox1.Items.RemoveAt(index);
                    }
                }

            }
            catch
            {
                MessageBox.Show("没有这个IP:port");
            }
        }
               
        //启动监听
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.start_listen();
            Listener.SERVER_LISTEN_PORT = (ushort)numericUpDown2.Value;
            numericUpDown2.ReadOnly = true;
            this.toolStripStatusLabel2.Text = "监听已启动    ";
            this.toolStripStatusLabel3.Text = "";
        }

        //停止监听
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            numericUpDown2.ReadOnly = false;
            try
            {
                lis.listenerRun = false;
                lis.Stop();
                this.toolStripStatusLabel2.Text = "监听已停止    ";
            }
            catch (NullReferenceException)
            { }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;

            this.label1.Text = "本主机IP是：" + GetMyIpAddress();
        }

        //连接
        private void start_listen()
        {
            lis = new Listener();
            lis.OnAddMessage += new EventHandler<AddMessageEventArgs>(this.AddMessage);
            lis.OnIpRemod += new EventHandler<AddMessageEventArgs>(this.IpRemo);
            lis.StartListener();
        }

        //获取本机IP
        private static string GetMyIpAddress()
        {
            IPAddress addr = new System.Net.IPAddress(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].Address);
            IPAddress[] tmp = Dns.GetHostByName(Dns.GetHostName()).AddressList;
            return addr.ToString();
        }

        //发送
        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0 && chatToIp == "" && chatToIp == null && listBox1.SelectedIndex < 0)
            {
                MessageBox.Show("请选择目标主机");
                return;
            }
            else if (textBox1.Text.Trim() == "")
            {
                MessageBox.Show("消息内容不能为空!", "错误");
                this.textBox1.Focus();
                return;
            }
            else
            {
                try
                {
                    sen = new Sender(chatToIp);
                    string txt = Sender.addHeader(textBox1.Text,Listener.DIRECT,"");
                    sen.Send(txt, (ushort)(chatToPort+10000));
                    string appendText;
                    appendText = "Me:       " + System.DateTime.Now.ToString() + Environment.NewLine + textBox1.Text + Environment.NewLine;

                    int txtGetMsgLength = this.richTextBox1.Text.Length;
                    this.richTextBox1.AppendText(appendText);
                    this.richTextBox1.Select(txtGetMsgLength, appendText.Length - Environment.NewLine.Length * 2 - textBox1.Text.Length);
                    this.richTextBox1.SelectionColor = Color.Green;
                    this.richTextBox1.ScrollToCaret();
                }
                catch
                { }
                this.textBox1.Text = "";
                this.textBox1.Focus();
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Clicks != 0)
            {
                if (listBox1.SelectedItem != null)
                {
                    //this.start_listen();
                    chatToIp = listBox1.SelectedItem.ToString().Split(':')[0];
                    chatToPort = UInt16.Parse(listBox1.SelectedItem.ToString().Split(':')[1]);
                    toolStripStatusLabel3.Text = "与" + chatToIp+":"+chatToPort+ "聊天中";
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
