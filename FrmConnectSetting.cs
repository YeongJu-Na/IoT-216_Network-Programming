using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChattingProgram
{
    public partial class FrmConnectSetting : Form
    {
        public FrmConnectSetting(string sAddrC, string sPortC)
        {
            InitializeComponent();
            tbIP.Text = sAddrC;
            tbPort.Text = sPortC;
        }

        private void FrmConnectSetting_Load(object sender, EventArgs e)
        {

        }
        public string ip;
        public string port;
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!isValidIP(tbIP.Text) || !isValidPort(tbPort.Text)) return;
            ip = tbIP.Text;
            port = tbPort.Text;
        }
       
        bool isNum(string str)
        {
             foreach(char c in str.ToCharArray())
            {
                if (!char.IsDigit(c)) return false;
            }
            return true;

        }
        bool isValidPort(string str)
        {
            if (!isNum(str)) return false;
            int val = int.Parse(str);
            if (val < 0 || val > 65535) return false;
            return true;
        }
        bool isValidIP(string str)
        {
            string[] sArr = str.Split('.');
            if (sArr.Length != 4) return false;
            for (int i = 0; i < 4; i++)
            {
                if (!isNum(sArr[i])) return false;
                int val = int.Parse(sArr[i]);
                if (val < 0 || val > 255) return false;

            }

            return true;
        }
    }
}
