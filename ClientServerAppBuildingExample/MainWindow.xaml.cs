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

namespace ClientServerAppBuildingExample
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public TcpClient Client { get; set; } = new TcpClient();
        public string ClientName { get; set; }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ConnectButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(serverIpTextBox.Text) || string.IsNullOrEmpty(portTextBox.Text) || string.IsNullOrEmpty(nameTextBox.Text))
            {
                MessageBox.Show("Не все поля заполнены!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Client == null)
                Client = new TcpClient();
            try
            {
                Client.Connect(IPAddress.Parse(serverIpTextBox.Text), int.Parse(portTextBox.Text));
            }
            catch(SocketException)
            {
                MessageBox.Show("Не удалось подключиться");
                return;
            }
            ClientName = nameTextBox.Text;
            Client.Client.Send(Encoding.UTF8.GetBytes(ClientName));

            ThreadPool.QueueUserWorkItem(ReceiveMessages);
        }

        private void ReceiveMessages(object state)
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int receiveSize = Client.Client.Receive(buffer);
                if (receiveSize <= 0)
                    return;

                string message = Encoding.UTF8.GetString(buffer, 0, receiveSize);
                if (message.Contains("&CL:"))
                {
                    string[] members = message.Remove(0, 4).Split('/');

                    Dispatcher.Invoke(() =>
                    {
                        membersComboBox.Items.Clear();
                        membersComboBox.Items.Add(new ComboBoxItem { Content = "All" });
                    });
                    foreach (var member in members)
                    {
                        if (member != ClientName)
                            Dispatcher.Invoke(() => membersComboBox.Items.Add(new ComboBoxItem { Content = member }));
                    }
                    Dispatcher.Invoke(() => membersComboBox.SelectedIndex = 0);
                }
                else
                    Dispatcher.Invoke(() => chatTextBox.AppendText(Encoding.UTF8.GetString(buffer, 0, receiveSize)));
            }
        }

        private void DisconnectButtonClick(object sender, RoutedEventArgs e)
        {
            Client.Close();
        }

        private void SendButtonClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(messageTextBox.Text))
            {
                if ((membersComboBox.SelectedItem as ComboBoxItem).Content.ToString() == "All")
                    Client.Client.Send(Encoding.UTF8.GetBytes($"{ClientName}: {messageTextBox.Text}\n"));
                else
                    Client.Client.Send(Encoding.UTF8.GetBytes($"{ClientName}: {messageTextBox.Text}~{(membersComboBox.SelectedItem as ComboBoxItem).Content.ToString()}"));

            }
        }
    }
}
