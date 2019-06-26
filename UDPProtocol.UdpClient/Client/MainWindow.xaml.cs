using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

            _client = new UdpClient(54321);
        }

        private UdpClient _client;

        private async void ButtonClick(object sender, RoutedEventArgs e)
        {
            if (_client == null)
                _client = new UdpClient(54321);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
            byte[] buffer = Encoding.UTF8.GetBytes("GETSCREENSHOT");
            await _client.SendAsync(buffer, buffer.Length);
            var receiveResult = await _client.ReceiveAsync();
            buffer = receiveResult.Buffer;

            using (FileStream stream = new FileStream("ServerScreenshot.jpg", FileMode.OpenOrCreate))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(buffer);
                }
            }

            ImageSourceConverter converter = new ImageSourceConverter();
            screenshotImage.Source = (ImageSource)converter.ConvertFromString("ServerScreenshot.jpg");

            _client.Close();
        }
    }
}
