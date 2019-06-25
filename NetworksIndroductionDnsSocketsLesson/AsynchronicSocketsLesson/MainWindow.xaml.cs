using System;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;

namespace AsynchronicSocketsLesson
{
    public partial class MainWindow : Window
    {
        // Объект сокета сервера
        TcpListener socketServer = null;
        // рабочий поток сервера
        Thread threadServer = null;
        // флаг запуска сервера
        bool boolIsStart = false;
        // список клиентов
        List<ClientInfo> listClients;

        public MainWindow()
        {
            InitializeComponent();

            Process currentProcess = Process.GetCurrentProcess();
            if (Process.GetProcessesByName(currentProcess.ProcessName).Length > 1)
            {
                MessageBox.Show("The instance of the process already exists");
                Close();
            }

            socketServer = null;
            threadServer = null;
            boolIsStart = false; // сервер не запущен
            comboBoxIpServer.Items.Add("0.0.0.0");
            comboBoxIpServer.Items.Add("127.0.0.1"); // localhost
            IPHostEntry ipServer = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var item in ipServer.AddressList)
            {
                comboBoxIpServer.Items.Add(item.ToString());
            }
            comboBoxIpServer.SelectedIndex = 0;
            listClients = new List<ClientInfo>();
        }

        private void ButtonStartClick(object sender, RoutedEventArgs e)
        {
            if (!boolIsStart) // запуск сервера
            {
                socketServer = new TcpListener(IPAddress.Parse(comboBoxIpServer.SelectedItem.ToString()), int.Parse(textPort.Text));
                // начало прослушивания порта сервера
                socketServer.Start(100);
                // запуск потока сервера
                threadServer = new Thread(ServerThreadProcedure);
                threadServer.Start(socketServer);

                buttonStart.Content = "Stop";
                boolIsStart = true;
            }
            else // остановка запущенного сервера
            {
                //boolIsStart = !boolIsStart;
                boolIsStart = false;

            }
        } // ButtonStartClick();

        void ServerThreadProcedure(object obj)
        {
            TcpListener serverSocket = (TcpListener)obj;
            while (true)
            {
                // ожидание клиента через асинхронный метод
                //  BeginAcceptTcpClient()
                IAsyncResult iAsyncResult = serverSocket.BeginAcceptTcpClient(AcceptClientProcedure, serverSocket);
                // ожидание завершения асинхронного
                //  соединения со сторонны клиента
                iAsyncResult.AsyncWaitHandle.WaitOne();
            }

        } // ServerThreadProcedure();
        public void SaveToLog(string str)
        {
            //textLog.Text += str;
            Dispatcher.Invoke(new Action(() => { textLog.AppendText(str); }));
        }
        // Ф-ция обратного вызова асинхронного события
        //  подключение клиента
        void AcceptClientProcedure(IAsyncResult iAsyncResult)
        {
            TcpListener serverSocket = (TcpListener)iAsyncResult.AsyncState;
            // обработка асинхронного события
            TcpClient client = serverSocket.EndAcceptTcpClient(iAsyncResult);
            SaveToLog("Клиент соединился с сервером\r\n");
            SaveToLog("Адрес клиента: " + client.Client.RemoteEndPoint.ToString() + "\n");

            // Запуск потока для работы с клиентом
            //  в пуле потоков процесса
            // Передаем рабочий объект (сокет) типа
            //  TcpClient
            ThreadPool.QueueUserWorkItem(ClientThreadProcedure, client);
        } // AcceptClientProcedure
        // рабочий потоковый метод работы с клиентами
        void ClientThreadProcedure(object obj)
        {
            TcpClient clientSocket = (TcpClient)obj;
            byte[] receiveBuffer = new byte[4 * 1024]; // 4 Kb
            // Необходимо реализовать протокол
            // общения сервера с клиентом
            // 1 действие - сервер ждет имя клиента
            int receiveSize = clientSocket.Client.Receive(receiveBuffer);
            string userName = Encoding.UTF8.GetString(receiveBuffer, 0, receiveSize);
            // 2 - ответ сервера: "Hello " + userName + "!"
            clientSocket.Client.Send(Encoding.UTF8.GetBytes("Hello " + userName + "!\n"));
            // Клиент подключен
            ClientInfo clientInfo = new ClientInfo
            {
                Name = userName,
                Client = clientSocket
            };
            listClients.Add(clientInfo);
            string clientsNames = "";
            foreach (var client in listClients)
                clientsNames += client.Name + "/";

            clientsNames = clientsNames.TrimEnd('/');

            foreach (var client in listClients)
                client.Client.Client.Send(Encoding.UTF8.GetBytes("&CL:" + clientsNames));

            while (true)
            {
                //clientSocket.ReceiveTimeout = 200;
                // получение сообщения от клиента для 
                //  дальнейшей рассылки клиентам по списку
                try
                {
                    receiveSize = clientSocket.Client.Receive(receiveBuffer);
                }
                catch (SocketException)
                {
                    break;
                }
                if (receiveSize <= 0)
                {
                    // связь разорвана клиентом
                    break;
                }
                string receiveMessage = Encoding.UTF8.GetString(receiveBuffer, 0, receiveSize);
                // ответ серверу
                foreach (var item in listClients)
                {
                    // проверка клиента на наличие связи с ним
                    if (item.Client.Client.Connected)
                    {
                        if (!receiveMessage.Contains("~"))
                            item.Client.Client.Send(receiveBuffer, receiveSize, SocketFlags.None);
                        else
                        {
                            string receiver = receiveMessage.Split('~')[1];
                            string message = receiveMessage.Split('~')[0] + "\n";
                            receiveBuffer = Encoding.UTF8.GetBytes(message);
                            if (item.Name == receiver)
                            {
                                item.Client.Client.Send(receiveBuffer, receiveBuffer.Length, SocketFlags.None);
                            }
                        }
                    }

                }

            }
            // 1
            listClients.Remove(clientInfo);
            clientsNames = "";
            foreach (var client in listClients)
                clientsNames += client.Name + "/";

            clientsNames = clientsNames.TrimEnd('/');

            foreach (var client in listClients)
                client.Client.Client.Send(Encoding.UTF8.GetBytes("&CL:" + clientsNames));
            //// 2
            //foreach (var item in listClients)
            //{
            //    if (item.Client == clientSocket)
            //    {
            //        listClients.Remove(item);
            //        break;
            //    }
            //}
        } // ClientThreadProcedure();
    } // class MainWindow
}
