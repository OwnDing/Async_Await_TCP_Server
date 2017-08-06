using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Blank_TCP_Server
{
    static class Program
    {
        static Mutex mutex = new Mutex(true, "TCPSERVER DingJianchen-2017-7-18");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!mutex.WaitOne(TimeSpan.FromSeconds(3), false))
            {
                Console.WriteLine("Another instance of the app is running. Bye!");
                Thread.Sleep(2000);
                return;
            }

            try
            {
                Console.WriteLine("Application is Running!");
            }
            finally
            {
                mutex.ReleaseMutex();
            }

            Application.Run(new tcpform());
        }
    }
}
