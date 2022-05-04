using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;

namespace TCP_Server
{
    class Sender
    {
        private string obj; //目标主机

        private delegate void SendMessageDelegate(string sendMessage,ushort rport);
        private SendMessageDelegate sendMessageDelegate;

        public Sender(string str)
        {
            obj = str;
        }

        public void Send(string str,ushort rport)
        {
            sendMessageDelegate = new SendMessageDelegate(SendMessage);

            IAsyncResult result = sendMessageDelegate.BeginInvoke(str.ToString(),rport, null, null);    //异步操作3
            sendMessageDelegate.EndInvoke(result);
        }

        public static string addHeader(string raw, int code, string extra)
        {
            string operation = "";
            if (code == Listener.DIRECT)
            {
                operation = Listener.S_DIRECT;
            }
            else if (code == Listener.CONNECT)
            {
                operation = Listener.S_CONNECT;
            }
            else if (code == Listener.DISCONNECT)
            {
                operation = Listener.S_DISCONNECT;
            }
            else if (code == Listener.TRANSFER)
            {
                operation = Listener.S_TRANSFER;
            }
            else if (code == Listener.REFRESH)
            {
                operation = Listener.S_REFRESH;
            }
            else
            {
                operation = "";
            }
            return operation +" "+ extra + "\n" + raw;
        }

        private void SendMessage(string sendMessage,ushort rport)
        {
            try
            {
                TcpClient tcpc = new TcpClient(obj, rport);
                NetworkStream tcpStream = tcpc.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(sendMessage + Environment.NewLine);
                tcpStream.Write(data, 0, data.Length);
                tcpStream.Close();
                tcpc.Close();
            }
            catch (Exception)
            {
                try
                {
                    MessageBox.Show("连接被目标主机拒绝");
                    IPEndPoint remove = new IPEndPoint(IPAddress.Parse(obj), rport - 10000);
                    Listener lis = Listener.dic[remove];
                    Listener.dic.Remove(remove);
                    AddMessageEventArgs argRe = new AddMessageEventArgs();
                    lis.ipRemove(argRe, remove);
                }
                catch (Exception exc) { }

            }
        }
    }
}
