using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace RVIP_41
{
    public partial class Form1 : Form
    {
        Socket sck;
        EndPoint eplLocal, epRemote;
        public Form1()
        {
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            InitializeComponent();
            textLocalIP.Text = GetLocalIP();
            textFriendsIP.Text = GetLocalIP();
        }
        //Метод, который получает сообщение там, куда его послали
        private void MessageCallBack(IAsyncResult aResult)
        {
            try
            {
                int size = sck.EndReceiveFrom(aResult, ref epRemote);
                if (size > 0)
                {
                    byte[] receiveData = new byte[1464];
                    receiveData = (byte[])aResult.AsyncState;

                    ASCIIEncoding eEncoding = new ASCIIEncoding();
                    string receiveMessage = eEncoding.GetString(receiveData);
                    //Сообщение, которое будет выведено
                    listMessage.Items.Add("Printer: " + receiveMessage);
                }
                byte[] buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
            }
            catch(Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }

        //Метод для получения IP компутера
        private string GetLocalIP()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());

            foreach(IPAddress ip in host.AddressList)
            {
                if(ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }
        //Соединяем 2 точки
        private void Startbtn_Click(object sender, EventArgs e)
        {
            try
            {
                //Соединить что
                eplLocal = new IPEndPoint(IPAddress.Parse(textLocalIP.Text), Convert.ToInt32(textLocalPort.Text));
                sck.Bind(eplLocal);
                //Соединить с чем
                epRemote = new IPEndPoint(IPAddress.Parse(textFriendsIP.Text), Convert.ToInt32(textFriendsPort.Text));
                sck.Connect(epRemote);
                //Метод который получает данные
                byte[] buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);

                Startbtn.Text = "Connected";
                Startbtn.Enabled = false;
                Sendbtn.Enabled = true;
                textMessage.Focus();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //Метод который отправляет сообщение
        private void Sendbtn_Click(object sender, EventArgs e)
        {
            try
            {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                byte[] msg = new byte[1500];
                msg = enc.GetBytes(textMessage.Text);

                sck.Send(msg);
                listMessage.Items.Add("You: " + textMessage.Text);
                textMessage.Clear();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
