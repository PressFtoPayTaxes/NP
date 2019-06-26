using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 12345;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, port);
            UdpClient client = new UdpClient(endPoint);

            while (true)
            {
                try
                {
                    // Тут возникает непонятная ошибка с переполнением буфера или очереди, с которой я так
                    //  и не смог разобраться
                    // Остальное должно работать
                    client.BeginReceive(ReceiveRequests, new UdpState { Client = client, EndPoint = endPoint });
                }
                catch(SocketException exception)
                {
                    MessageBox.Show($"Ошибка сокета({exception.ErrorCode}): {exception.Message}");
                    return;
                }
            }
        }

        static void ReceiveRequests(IAsyncResult result)
        {
            UdpClient client = ((UdpState)result.AsyncState).Client;
            IPEndPoint endPoint = ((UdpState)result.AsyncState).EndPoint;

            byte[] buffer = client.EndReceive(result, ref endPoint);
            string message = Encoding.UTF8.GetString(buffer);
            if (message == "GETSCREESHOT")
            {
                ThreadPool.QueueUserWorkItem(SendScreenshot, new UdpState { Client = client, EndPoint = endPoint });
            }
        }

        private static void SendScreenshot(object state)
        {
            IPEndPoint endPoint = (state as UdpState).EndPoint;
            UdpClient client = (state as UdpState).Client;

            var bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (var graphic = Graphics.FromImage(bitmap))
            {
                graphic.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
            }
            bitmap.Save("Screenshot.jpg");

            using (var stream = new FileStream("Screenshot.jpg", FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    byte[] buffer = reader.ReadBytes((int)stream.Length);
                    client.Send(buffer, (int)stream.Length, endPoint);
                }
            }
        }
    }
}
