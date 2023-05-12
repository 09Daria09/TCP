using System;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

namespace TCP
{
    public partial class Form1 : Form
    {
        [Serializable]
        public struct Mes
        {
            public string mes; // текст сообщения
            public string host; // имя хоста
            public string user; // имя пользователя
        }
        public SynchronizationContext uiContext { get; set; }
        Client client1;

        private static List<Client> childForms = new List<Client>();
        public Form1()
        {
            InitializeComponent();
            uiContext = SynchronizationContext.Current;
            WaitClientQuery();
        }
        // ********************************* серверная часть ************************************************
        private async void WaitClientQuery()
        {
            await Task.Run(() =>
            {
                try
                {
                    TcpListener listener = new TcpListener(
                    IPAddress.Any, 49152);
                    listener.Start(); // Запускаем ожидание входящих запросов на подключение
                    while (true)
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        ReadMessage(client);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Сервер: " + ex.Message);
                }
            });

        }

        private async void ReadMessage(TcpClient client2)
        {
            await Task.Run(() =>
            {
                try
                {
                    NetworkStream netstream = client2.GetStream();
                    byte[] arr = new byte[client2.ReceiveBufferSize];
                    int len = netstream.Read(arr, 0, client2.ReceiveBufferSize);
                    if (len > 0)
                    {
                        MemoryStream stream = new MemoryStream(arr);
                        BinaryFormatter formatter = new BinaryFormatter();
                        Mes m = (Mes)formatter.Deserialize(stream);

                        uiContext.Send(d => listBox1.Items.Add(m.host), null); // добавляем в список имя клиента
                        uiContext.Send(d => listBox1.Items.Add(m.user), null);
                        uiContext.Send(d => listBox1.Items.Add(m.mes), null);

                        foreach (Client childForm in childForms)
                        {
                            uiContext.Send(d => childForm.AddMessageToListBox(m.user), null);
                            uiContext.Send(d => childForm.AddMessageToListBox(m.mes), null); 
                        }
                        stream.Close();
                    }
                    netstream.Close();
                    client2.Close(); // закрываем TCP-подключение и освобождаем все ресурсы, связанные с объектом TcpClient.
                }
                catch (Exception ex)
                {
                    client2.Close(); // закрываем TCP-подключение и освобождаем все ресурсы, связанные с объектом TcpClient.
                    MessageBox.Show("Сервер: " + ex.Message);
                }
            });

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Client childForm = new Client(textBox2.Text, uiContext, textBox1.Text);
            childForms.Add(childForm);
            childForm.Show();
        }

    }
}
