using Blank_TCP_Server.Servers.Function;
using System;
using System.Windows.Forms;

namespace Blank_TCP_Server.Views
{
    public partial class DataTransfer : Form
    {
        public DataTransfer()
        {
            InitializeComponent();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            rtbInput.Text = string.Empty;
            rtbOutput.Text = string.Empty;
        }

        private void btnStringToHex_Click(object sender, EventArgs e)
        {
            if(rtbInput.Text == string.Empty){
                MessageBox.Show("Input!");
                return;
            }

            rtbOutput.Text = StaticMethod.StringToHex(rtbInput.Text);
        }

        private void btnHexToString_Click(object sender, EventArgs e)
        {
            if (rtbInput.Text == string.Empty)
            {
                MessageBox.Show("Input!");
                return;
            }
            rtbOutput.Text = StaticMethod.HexToString(rtbInput.Text);
        }
    }
}
