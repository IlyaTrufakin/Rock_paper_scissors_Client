using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Rock_paper_scissors_Client
{
    internal class ClientCommunication
    {
        private Socket? clientSocket;

        public string ConnectToServer(string ipAddress, string port)
        {
            try
            {
                if (IsConnected())
                {
                    return "Соединение с сервером уже установлено";
                }
                IPEndPoint endpoint = SocketInit(ipAddress, port);
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(endpoint);
                // Устанавливаем тайм-аут 1 секунда, на прием данных
                //clientSocket.ReceiveTimeout = 1000;
                return "Соединение с сервером установлено";
            }
            catch (Exception ex)
            {
                return "Ошибка соединения с сервером: " + ex.Message;
            }
        }

        public bool IsConnected()
        {
            return clientSocket != null && clientSocket.Connected;
        }


        public async Task<string> SendMessageAndReceiveResponseAsync(string message)
        {
            try
            {
                if (clientSocket != null && clientSocket.Poll(0, SelectMode.SelectWrite))
                {
                    // Сокет готов для отправки данных
                    byte[] sendData = Encoding.Unicode.GetBytes(message);
                    await Task.Run(() => clientSocket.Send(sendData)); // Отправка сообщения в отдельном потоке

                    // Ожидание и прием ответа в отдельном потоке
                    return await Task.Run(() => ReceiveMessage());
                }
                else
                {
                    // Сокет не готов для отправки данных
                    return "Сокет не готов для отправки данных";
                }
            }
            catch (Exception ex)
            {
                return "Ошибка отправки сообщения/получения ответа: " + ex.Message;
            }


        }


        private string ReceiveMessage()
        {
            try
            {
                byte[] receiveData = new byte[256];
                StringBuilder receivedString = new StringBuilder();
                int bytesReceived;

                while (true)
                {
                    bytesReceived = clientSocket.Receive(receiveData);
                    receivedString.Append(Encoding.Unicode.GetString(receiveData, 0, bytesReceived));

                    if (bytesReceived < receiveData.Length)
                    {
                        break;
                    }
                }

                return receivedString.ToString();
            }
            catch (Exception ex)
            {
                return "Ошибка получения ответа: " + ex.Message;
            }
        }


        private IPEndPoint SocketInit(string ipAddress, string port)
        {
            try
            {
                if (!int.TryParse(port, out int portNumber))
                {
                    throw new ArgumentException("Порт задан неверно (должен быть числом)");
                }

                return new IPEndPoint(IPAddress.Parse(ipAddress), portNumber);
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка инициализации сокета: " + ex.Message);
            }
        }
    }
}
