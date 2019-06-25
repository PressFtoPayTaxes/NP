using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AsynchronicSockets
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new DataContext())
            {
                if (context.Streets.Count() == 0)
                {
                    context.Streets.AddRange(new List<Street>
                    {
                        new Street { Postcode = "Z05T7F0", Name = "УЛИЦА Е 10, дом 11" },
                        new Street { Postcode = "Z10F7D2", Name = "ПЕРЕУЛОК Жарокова, здание 1А" },
                        new Street { Postcode = "Z10H5F9", Name = "ПРОСПЕКТ Республика, здание 54/2" },
                        new Street { Postcode = "Z11E7B0", Name = "ПЕРЕУЛОК Жарокова, здание 10С" },
                        new Street { Postcode = "Z01F8M4", Name = "МИКРОРАЙОН Жастар, УЛИЦА Александр Бараев, сооружение 16Е" },
                        new Street { Postcode = "Z01G8A3", Name = "ЖИЛОЙ МАССИВ Промышленный, УЛИЦА Қапал, здание 8Б" },
                        new Street { Postcode = "Z10M2Y8", Name = "ПЕРЕУЛОК Второй Алматинский, дом 30/1" },
                        new Street { Postcode = "Z10M3C7", Name = "УЛИЦА Первая Алматинская, дом 55" },
                        new Street { Postcode = "Z11E4H1", Name = "ПЕРЕУЛОК Второй Алматинский, дом 10Б" },
                        new Street { Postcode = "Z11F9F5", Name = "ПЕРЕУЛОК Второй Алматинский, дом 16В" },
                        new Street { Postcode = "Z05T7K8", Name = "ЖИЛОЙ МАССИВ Ильинка, УЛИЦА Е 586, дом 16" },
                        new Street { Postcode = "Z00E4T8", Name = "ЖИЛОЙ МАССИВ Энергетик, УЛИЦА Аскара Токпанова, дом 46" },
                        new Street { Postcode = "Z01D2F1", Name = "ЖИЛОЙ МАССИВ Железнодорожный, УЛИЦА Ащысай, дом 5/3" },
                        new Street { Postcode = "Z00K6X6", Name = "УЛИЦА Кайрата Рыскулбекова, дом 2/2" },
                        new Street { Postcode = "Z05D6E6", Name = "ЖИЛОЙ МАССИВ Комсомольский, УЛИЦА Ақбаян, дом 10" },
                        new Street { Postcode = "Z05D6K9", Name = "Нур-Султан, ЖИЛОЙ МАССИВ Комсомольский, УЛИЦА Ақбаян, дом 5" },
                    });

                    context.SaveChanges();
                }
            }

            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345));
            Console.WriteLine("Ожидаем подключения...");
            serverSocket.Listen(100);


            serverSocket.BeginAccept(AcceptClients, serverSocket);

        }

        static void AcceptClients(IAsyncResult result)
        {
            var serverSocket = result.AsyncState as Socket;
            var clientSocket = serverSocket.EndAccept(result);
            Console.WriteLine("Клиент подключен!\nОжидаем запроса...");

            ThreadPool.QueueUserWorkItem(ReceiveInfo, clientSocket);
        }

        static void ReceiveInfo(object state)
        {
            Socket clientSocket = state as Socket;

            while (clientSocket.Connected)
            {
                byte[] buffer = new byte[1024];
                int bytes = clientSocket.Receive(buffer);
                if (bytes <= 0)
                {
                    Console.WriteLine("Ошибка! Данные не получены");
                    return;
                }

                Console.WriteLine("Запрос получен! Отправляем ответ...");
                string postcode = Encoding.UTF8.GetString(buffer, 0, bytes);


                bool isFound = false;
                using (var context = new DataContext())
                {
                    foreach (var street in context.Streets)
                    {
                        if (street.Postcode.ToUpper() == postcode.ToUpper())
                        {
                            buffer = Encoding.UTF8.GetBytes(street.Name);
                            clientSocket.Send(buffer);

                            isFound = true;
                        }
                    }
                }

                if (!isFound)
                    clientSocket.Send(Encoding.UTF8.GetBytes("nothing"));
            }

        }
    }
}
