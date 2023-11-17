using Blank_TCP_Server.Servers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Blank_TCP_Server.Servers.Function;
using Microsoft.VisualBasic;
using Blank_TCP_Server.Views;
using System.Runtime.InteropServices;
using Blank_TCP_Server.Servers.AsyncAwaitServer;
using Blank_TCP_Server.Function;

namespace Blank_TCP_Server
{
    public partial class tcpform : Form
    {
        AsyncAwaitServer aas;

        public tcpform()
        {
            InitializeComponent();
            txtipport.Text = Properties.Settings.Default.ipport;
            txtTimer.Text = Properties.Settings.Default.beeptime;
            Console.Title = "Tcp Server";
            // 隐藏终端显示
            ConsoleWindow.HideConsoleWindow();
        }
        #region btn
        private void btn_setting_Click(object sender, EventArgs e)
        {
            if (txtipport.Text.Length < 3 || txtTimer.Text=="")
            {
                MessageBox.Show("請設置正確的IP,PORT,Timer！");
                return;
            }

            Properties.Settings.Default.ipport = txtipport.Text;
            Properties.Settings.Default.beeptime = txtTimer.Text;
            Properties.Settings.Default.Save();
        }

        private void btn_timersend_Click(object sender, EventArgs e)
        {
            if (txtSendData.Text == string.Empty)
            {
                MessageBox.Show("please input something in above textbox！");
                return;
            }
            if (btn_run.Text == "Stop")
            {
                if (timer1.Enabled == false)
                {
                    Console.WriteLine("timer started!");
                    timer1.Interval = Convert.ToInt32(txtTimer.Text);
                    timer1.Enabled = true;
                    tsslInfo.Text = "Auto Sending...";
                    txtSendData.Enabled = false;
                    return;
                }
            }
            Console.WriteLine("timer stoped!");
            tsslInfo.Text = "Ready!";
            timer1.Enabled = false;
            txtSendData.Enabled = true;
        }

        private void btn_run_Click(object sender, EventArgs e)
        {
            if (txtipport.Text.Length < 3 )
            {
                MessageBox.Show("請設置正確的IP和TIME！");
                return;
            }

            if (btn_run.Text == "Start")
            {
                btn_run.Text = "Stop";
                var th = new Thread(run)
                {
                    Priority = ThreadPriority.Highest
                };
                th.Start();
                txtipport.Enabled = false;
                txtTimer.Enabled = false;
                txtMaxConnections.Enabled = false;
            }
            else
            {
                if (aas != null)
                {
                    aas.Stop();
                }
                btn_run.Text = "Start";
                txtipport.Enabled = true;
                txtTimer.Enabled = true;
                txtMaxConnections.Enabled = true;
            }
            
        }
        private void btn_Send_Click(object sender, EventArgs e)
        {
            string data;
            if (txtSendData.Text != string.Empty)
            {
                data = txtSendDataToAll.Text;
            }
            else
            {
                data = "hello from server!";
            }

            byte[] sendData;
            if (rbtnH16.Checked == true)
            {
                sendData = StaticMethod.StringToByteArray(data);
            }
            else
            {
                sendData = Encoding.ASCII.GetBytes(data);
            }
            aas.SendToAll(sendData);
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout fa = new FormAbout();
            fa.ShowDialog();
        }
        private void transferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTransfer dt = new DataTransfer();
            dt.Show();
        }
        #endregion

        #region start server
        private void run()
        {
            try
            {
                int port = Convert.ToInt32(txtipport.Text);
                int max = Convert.ToInt32(txtMaxConnections.Text);
                aas = new AsyncAwaitServer(port, max);
                string msg = "Server listening on port " + port.ToString() + "...\n";
                Console.ForegroundColor = (ConsoleColor)((60 - 1) % 16);
                Console.WriteLine(msg);
                msg = "Timer Invert:" + timer1.Interval;
                Console.WriteLine(msg);
                aas.Eventlistview += UpdateListView;
                aas.Run();                              
                //aas.isStop = true;
                //Console.WriteLine("Server has Stoped!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: SocketAsyncServer <port> [numConnections] [bufferSize].");
            Console.WriteLine("\t<port> Numeric value for the listening TCP port.");
            Console.WriteLine("\t[numConnections] Numeric value for the maximum number of incoming connections.");
            Console.WriteLine("\t[bufferSize] Numeric value for the buffer size of incoming connections.");
        }
        #endregion

        #region send
        private void timer1_Tick(object sender, EventArgs e)
        {           
            try
            {
                string data = txtSendData.Text;

                aas.SendToAll(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void tsmiSend_Click(object sender, EventArgs e)
        {
            var selectedItems =lvClients.SelectedItems;
            if (selectedItems.Count > 0)
            {
                string datagram = Microsoft.VisualBasic.Interaction.InputBox("please input something", "SendToTcpClient", string.Empty, -1, -1);
                byte[] sendData;
                if (rbtnH16.Checked == true)
                {
                    sendData = StaticMethod.StringToByteArray(datagram);
                }
                else
                {
                    sendData = Encoding.ASCII.GetBytes(datagram);
                }
                for (int i = 0; i < selectedItems.Count; i++)
                {
                    string ip = selectedItems[i].SubItems[1].Text + ":" + selectedItems[i].SubItems[2].Text;
                    aas.SendToSelectedClient(ip, sendData);
                }
            }

        }
        #endregion

        #region Update View From Socket
        private void UpdateListView(string ip, AsyncAwaitServer.ConnectionStatus status)
        {
            lvClients.Invoke((MethodInvoker)delegate {
                if (status==AsyncAwaitServer.ConnectionStatus.add)
                {
                    ListViewItem lvi;
                    lvi = new ListViewItem(" Connected", 0);
                    var values = ip.Split(':');
                    lvi.SubItems.Add(ip.Substring(0, ip.Length - values[values.Count() - 1].Length - 1));
                    lvi.SubItems.Add(values[values.Count() - 1]);
                    lvClients.Items.Add(lvi);
                    string msg = "Connections:" + lvClients.Items.Count.ToString();
                    SetStatus(msg);
                }
                else
                {
                    for (int i = lvClients.Items.Count - 1; i >= 0; i--)
                    {
                        string s=lvClients.Items[i].SubItems[1].Text+":" + lvClients.Items[i].SubItems[2].Text;
                        if (s==ip)
                        {
                            lvClients.Items[i].Remove();
                        }
                    }
                    string msg = "Connections:" + lvClients.Items.Count.ToString();
                    SetStatus(msg);
                }
            });
        }

        delegate void SetStatusCallback(String text);
        private void SetStatus(String text)
        {
            if (statusStrip1.InvokeRequired)
            {
                SetStatusCallback d = new SetStatusCallback(SetStatus);
                this.Invoke(d, new object[] { text});
            }
            else
            {
                tsslConnections.Text = text;
            }
        }
        #endregion        

        #region Hidden application to tray
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private void tcpform_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Visible = true;
                //notifyIcon1.ShowBalloonTip(3000);
                this.ShowInTaskbar = false;

                var handle = GetConsoleWindow();

                // Hide
                ShowWindow(handle, SW_HIDE);
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            showForm();
        }
        private void showForm()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            notifyIcon1.Visible = false;

            var handle = GetConsoleWindow();
            // Show
            ShowWindow(handle, SW_SHOW);
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm();
        }

        #endregion

        private void tcpform_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult userAnswer = MessageBox.Show("Do you wish to close ALL Forms?", "Close",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (userAnswer != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            if (aas != null)
            {
                aas.Stop();
                aas.Eventlistview -= UpdateListView;
            }

            aas = null;
            this.Dispose();
        }
    }
}
