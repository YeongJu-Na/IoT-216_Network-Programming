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

namespace ChattingProgram
{
    public partial class FrmChat : Form
    {
        public FrmChat()
        {
            InitializeComponent();
        }

        byte[] bAddr = { 127, 0, 0, 1 };
        string sPort = "9000";

        string sAddrC = "";    //연결된 remoteEndPoint
        string sPortC = "";

        Socket sockServer = null;
        Socket sock = null;
        Thread threadServer = null;
        Thread threadRead = null;

        delegate void cbAddText(string str, int chk);

        void AddText(string str, int chk)   //0:둘다 1:send 2:receive
        {
            if (tbReceived.InvokeRequired)
            {
                cbAddText cb = new cbAddText(AddText);
                Invoke(cb, new object[] { str,chk });
            }
            else
            {
                if(chk == 0)
                {
                    tbReceived.AppendText(str + '\n');
                    tbSent.AppendText(str + '\n');
                }
                else if (chk == 1)
                {
                    tbReceived.AppendText("\n");
                    tbSent.AppendText(str + '\n');
                }
                else
                {
                    tbReceived.AppendText(str + '\n');
                    tbSent.AppendText("\n");
                }
                //tbReceived.AppendText(str+'\n');
            }
            
        }

        private void FrmChat_Load(object sender, EventArgs e)
        {
            ServerStart();
        }
        void ServerStart()
        {
            sockServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            threadServer = new Thread(ServerProcess);
            threadServer.IsBackground = true;    //주 스레드 종료시 함께 종료
            threadRead = new Thread(ReadProcess);
            threadRead.IsBackground = true;
            threadServer.Start();
            tbReceived.AppendText("연결 대기 상태"+'\n');
            tbSent.AppendText("연결 대기 상태"+'\n');
        }

        void ServerProcess()    //bind, listen, accept
        {
            IPAddress ip = new IPAddress(bAddr);
            IPEndPoint ep = new IPEndPoint(ip, int.Parse(sPort));
            sockServer.Bind(ep);
            sockServer.Listen(50000);
            //try
            {
                while (true)
                {
                    sock = sockServer.Accept();
                    if (sock != null)
                    {
                        string[] sArr=sock.RemoteEndPoint.ToString().Split(':');
                        sAddrC = sArr[0];
                        sPortC = sArr[1];
                        AddText(sock.RemoteEndPoint.ToString() + "와 연결되었습니다.",0);
                    }
                    threadRead.Start();
                }
            }
            //catch(Exception e)
            {
            //    MessageBox.Show(e.Message);
            }
        }

        byte[] stringToIP(string str) //"127.0.0.1"
        {
            byte[] ret = new byte[4];
            string[] sArr = str.Split('.');
            for (int i = 0; i < 4; i++) ret[i] =Byte.Parse(sArr[i]);
            return ret;
        }

        void ReadProcess()  //연결된 소켓에 대해 읽어올 데이터 있을 시
        {
            while (true)
            {
                if(sock!=null && sock.Connected && sock.Available > 0)
                {
                    byte[] bArr = new byte[sock.Available];
                    sock.Receive(bArr);
                    AddText(Encoding.Default.GetString(bArr),2);
                }
            }
        }
        

        void SendText(string str)
        {

            if(sock==null || !sock.Connected)
            {
                MessageBox.Show("연결 정보가 없습니다.");
                return;
            }
            try
            {
                sock.Send(Encoding.Default.GetBytes(str));
                //Send완료시 
                AddText(str, 1);
                tbSend.Text = "";
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (tbSend.Text == "") return;
            SendText(tbSend.Text);
        }

        private void FrmChat_FormClosing(object sender, FormClosingEventArgs e)
        {
            ConnectInit();
        }

        private void mnuNewConnect_Click(object sender, EventArgs e)
        {
            FrmConnectSetting dlg = new FrmConnectSetting(sAddrC, sPortC);

            if (dlg.ShowDialog()== DialogResult.OK)
            {
                try
                {
                    sAddrC = dlg.ip;
                    sPortC = dlg.port;
                    ConnectInit();
                    sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sock.Connect(sAddrC, int.Parse(sPortC));
                    threadRead = new Thread(ReadProcess);
                    threadRead.Start();
                    threadRead.IsBackground = true;     
                    AddText(sock.RemoteEndPoint.ToString() + "와 연결되었습니다.", 0);

                }
                catch (Exception e1)
                {
                    MessageBox.Show(e1.Message);
                }
            }
        }

        void ConnectInit()
        {
            if (sockServer != null) sockServer.Close();
            if (sock != null) sock.Close();
            if (threadServer != null) threadServer.Abort();
            if (threadRead != null) threadRead.Abort();
            tbReceived.AppendText("----------------------------------");
            tbSent.AppendText("----------------------------------");
        }

        private void mnuWait_Click(object sender, EventArgs e)
        {
            
            ConnectInit();
            ServerStart();
        }
    }
}