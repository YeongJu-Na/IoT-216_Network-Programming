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

namespace ComTest
{
    public partial class FormServer : Form
    {
        public FormServer()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            // 1. Socket생성: 주소x
            // 2. Socket Open - Connection 수립 - 주소 부여
            // 3. 메세지 전송: Message -문자열 외에 이미지나 동영상도 가능. 단 양축이 서로 약속된 약에 의해서
            //==> 프로토콜 제정

            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Connect(tbIP.Text, int.Parse(tbPort.Text));
            string str = tbClient.Text;
            byte[] bArr = Encoding.Default.GetBytes(str);   //윈도우즈의 현재 코드체계로

            sock.Send(bArr);

        }

        TcpListener listener = null;
        string MainMsg = "";     // tbServer.Text
        Thread thread = null;

        delegate void cbAddText(string str); //string str을 받아 tbServer에 출력하는 콜백함수

        void AddText(string str)    //tbServer에 str출력하는 함수, thread내부에서 호출될 함수
        {
            if (tbServer.InvokeRequired)    //cross thread로 인해 invoke필요한 경우
            {
                cbAddText at = new cbAddText(AddText);  //함수의 포인터
                Invoke(at, new object[] { str });
            }
            else tbServer.Text += str;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (thread == null)
            {
                thread = new Thread(ServerProcess);
                thread.Start();
            }
            timer1.Enabled = true;
            sbServerMsg.Text = "Listener Started";
        }

        private void ServerProcess()
        {
            while (true)
            {
                if (listener == null)
                {
                    listener = new TcpListener(int.Parse(tbServerPort.Text));
                    listener.Start();
                }
                if (listener.Pending())
                {
                    Socket socket = listener.AcceptSocket();
                    byte[] bArr = new byte[socket.Available];   //while(ns.DataAvailable) 대신
                    int n = socket.Receive(bArr);
                    AddText(Encoding.Default.GetString(bArr, 0, n));

                    //EndPoint ep = socket.RemoteEndPoint;    //127.0.0.1 :포트번호는 정확x
                    
                    IPEndPoint ep = (IPEndPoint)socket.RemoteEndPoint;
                    sbServerLable2.Text = $"{ep.Address}:{ep.Port}";    //ep.port는 세션 번호

                    /*
                    TcpClient tcp = listener.AcceptTcpClient();
                    EndPoint ep = tcp.Client.RemoteEndPoint;
                    sbServerLable2.Text = ep.ToString();

                    NetworkStream ns = tcp.GetStream();
                    byte[] bArr = new byte[200];

                    while (ns.DataAvailable)
                    {
                        int n = ns.Read(bArr, 0, 200);
                        //MainMsg += Encoding.Default.GetString(bArr, 0, n);
                        AddText(Encoding.Default.GetString(bArr, 0, n));
                    }
                    */
                }
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            thread.Abort();
            thread = null;
            timer1.Stop();
            sbServerMsg.Text = "Server stopped";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            tbServer.Text += MainMsg;
            //MainMsg = "";
        }

        private void FormServer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(thread!=null)    thread.Abort();
        }
    }
}
