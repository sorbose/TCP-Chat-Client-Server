using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TCP_Server
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form1=new Form1();
            Application.Run(form1);
        }
        public static Form1 form1;
    }
}
