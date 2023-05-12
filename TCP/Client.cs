using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static TCP.Form1;
using System.Runtime.Remoting.Contexts;
using System.Threading;

namespace TCP
{
    public partial class Client : Form
    {
        string userName = null;
        public SynchronizationContext uiContext;
        string port;

        public Client(string name, SynchronizationContext context, string p)
        {
            InitializeComponent();;
            userName = name;
            uiContext = context;
            port = p;

        }
        public void AddMessageToListBox(string message)
        {
            listBox1.Items.Add(message);
        }
        private async void ConnectAndSend()
        {
            await Task.Run(() =>
            {
                try
                {
                    TcpClient client = new TcpClient(port /* имя хоста */, 49152 /* порт */);
                    NetworkStream netstream = client.GetStream();
                    MemoryStream stream = new MemoryStream();
                    BinaryFormatter formatter = new BinaryFormatter();
                    Mes m = new Mes();
                    m.mes = textBox1.Text; // текст сообщения
                    m.user = userName + ": "; // имя пользователя
                    m.host = Dns.GetHostName(); // имя хоста
                    //uiContext.Send(d => listBox1.Items.Add(m.user), null);
                   // uiContext.Send(d => listBox1.Items.Add(m.mes), null);
                    formatter.Serialize(stream, m); // выполняем сериализацию
                    byte[] arr = stream.ToArray(); // записываем содержимое потока в байтовый массив
                    stream.Close();
                    netstream.Write(arr, 0, arr.Length); // записываем данные в NetworkStream.
                    netstream.Close();
                    client.Close(); // закрываем TCP-подключение и освобождаем все ресурсы, связанные с объектом TcpClient.

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Клиент: " + ex.Message);
                }
            });
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            ConnectAndSend();
        }
    }
}
