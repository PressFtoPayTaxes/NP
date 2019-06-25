using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        }

        private Socket _socket;

        private void SendButtonClick(object sender, RoutedEventArgs e)
        {
            _socket.Send(Encoding.UTF8.GetBytes(postcodeTextBox.Text));

            ThreadPool.QueueUserWorkItem(ReceiveInfo);
        }

        private void ReceiveInfo(object state)
        {
            byte[] buffer = new byte[1024];
            int bytes = _socket.Receive(buffer);

            string message = Encoding.UTF8.GetString(buffer, 0, bytes);
            if (message == "nothing")
                Dispatcher.Invoke(() => streetNameTextBox.Text = "Не найдено");
            else
                Dispatcher.Invoke(() => streetNameTextBox.Text = message);
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _socket.Close();
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345));
            }
            catch (SocketException)
            {
                MessageBox.Show("Не удалось подключиться к серверу");
                return;
            }
            MessageBox.Show("Вы подключены к серверу");
            connectButton.IsEnabled = false;
            sendButton.IsEnabled = true;
        }
    }
}
