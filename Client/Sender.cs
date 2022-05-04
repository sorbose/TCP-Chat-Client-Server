using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    class Sender
    {
        private string remote;//目标主机

        private delegate void SendMessageDelegate(string sendMessage);
        private SendMessageDelegate sendMessageDelegate;

        public static readonly int DIRECT = 0;
        public static readonly int CONNECT = 1;
        public static readonly int DISCONNECT = 2;
        public static readonly int TRANSFER = 3;
        public static readonly int REFRESH = 4;
        public static readonly string S_DIRECT = "DIRECT";
        public static readonly string S_CONNECT = "CONNNECT";
        public static readonly string S_DISCONNECT = "DISCONN";
        public static readonly string S_TRANSFER = "TRANSFER";
        public static readonly string S_REFRESH = "REFRESH";

        public Sender(string str)
        {
            remote = str;
        }

        public void Send(string text)
        {
            sendMessageDelegate = new SendMessageDelegate(SendMessage);
            IAsyncResult result = sendMessageDelegate.BeginInvoke(text.ToString(), null, null);    //异步操作3
            sendMessageDelegate.EndInvoke(result);
        }

        private void SendMessage(string sendMessage)
        {
            try
            {
                TcpClient tcpc = Client.tcpClient;
                Socket socket = tcpc.Client;
                NetworkStream tcpStream = tcpc.GetStream();
                Byte[] data = Encoding.UTF8.GetBytes(sendMessage + Environment.NewLine);
                tcpStream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Program.client.setStatus(ex.Message);
            }
        }

        public static string addHeader(String raw,int code,string extra)
        {
            string operation="";
            if (code == DIRECT)
            {
                operation = S_DIRECT;
            }
            else if (code == CONNECT)
            {
                operation = S_CONNECT;
            }else if (code == DISCONNECT)
            {
                operation = S_DISCONNECT;
            }else if (code == TRANSFER)
            {
                operation = S_TRANSFER;
            }else if (code == REFRESH)
            {
                operation = S_REFRESH;
            }
            else
            {
                operation = "";
            }
            return operation+" " + extra + "\n" + raw;
        }
    }
}
