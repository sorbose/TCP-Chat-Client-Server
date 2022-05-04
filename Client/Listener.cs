using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public class AddMessageEventArgs : EventArgs
    {
        public string mess;   //存放要显示的内容
    }

    class Listener
    {
        private delegate void ReceiveMessageDelegate(out string receiveMessage);
        private ReceiveMessageDelegate receiveMessageDelegate;

        private Thread th;
        private TcpListener tcpl;
        public bool listenerRun = true;//是否启动
        public event EventHandler<AddMessageEventArgs> OnAddMessage;

        public ushort lport;
        public Listener(ushort localListenport)
        {
            lport = localListenport;
        }
        //启动另一个线程开始监听
        public void StartListener()
        {
            th = new Thread(new ThreadStart(Listen));
            th.Start();
        }
        //停止监听
        public void Stop()
        {
            tcpl.Stop();
            th.Abort();
        }
        private void Listen()
        {
            string receiveString;
            receiveMessageDelegate = new ReceiveMessageDelegate(ReceiveMessage);
            IAsyncResult result = receiveMessageDelegate.BeginInvoke(out receiveString, null, null);
            receiveMessageDelegate.EndInvoke(out receiveString, result);
        }

        private string removeHeader(string raw, out int code, out IPEndPoint transferTo)
        {
            code = -1;
            transferTo = null;
            string[] txt = raw.Split(new char[] { '\n' }, 2);
            string[] header = txt[0].Split(new char[] { ' ' });
            string operation = header[0];
            if (operation == Sender.S_DIRECT)
            {
                code = Sender.DIRECT;
            }
            else if (operation == Sender.S_REFRESH)
            {
                code = Sender.REFRESH;
            }
            else if (operation == Sender.S_TRANSFER)
            {
                code = Sender.TRANSFER;
                string ip = header[1].Split(':')[0];
                string port = header[1].Split(':')[1];
                transferTo = new IPEndPoint(IPAddress.Parse(ip), Int32.Parse(port));
            }
            if (txt.Length > 1)
            {
                return txt[1];
            }
            else
            {
                return "";
            }
        }

        private void ReceiveMessage(out string receiveMessage)
        {
            receiveMessage = "";
            try
            {
                IPAddress addr = new IPAddress(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].Address);
                IPEndPoint ipLocalEndPoint = new IPEndPoint(addr, lport);
                tcpl = new TcpListener(ipLocalEndPoint);
                tcpl.Start();
                while (true)
                {
                    Socket s = tcpl.AcceptSocket();
                    string remote = s.RemoteEndPoint.ToString();
                    Byte[] stream = new Byte[1024];
                    int i = s.Receive(stream);
                    string raw = Encoding.UTF8.GetString(stream);
                    int code;
                    string txt;
                    txt = removeHeader(raw, out code, out IPEndPoint ipep);
                    if (code == Sender.REFRESH)
                    {
                        Program.client.refreshListBox(txt);
                    }else if (code == Sender.TRANSFER)
                    {
                        string msg = "user<" + ipep.ToString() + ">" + txt + Environment.NewLine;
                        AddMessageEventArgs arg = new AddMessageEventArgs();
                        arg.mess = msg;
                        OnAddMessage(this, arg);
                    }
                    else
                    {
                        string msg = "server<" + remote + ">" + txt + Environment.NewLine;
                        AddMessageEventArgs arg = new AddMessageEventArgs();
                        arg.mess = msg;
                        OnAddMessage(this, arg);
                    }
                }
            }
            catch (System.Security.SecurityException)
            {
                MessageBox.Show("防火墙禁止连接");
            }
            catch (Exception ex)
            {
                Program.client.setStatus(ex.Message);
            }
        }
    }
}
