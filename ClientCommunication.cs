using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Rock_paper_scissors_Client
{
    public class ClientCommunication
    {
        private Socket? clientSocket;
        private const int bufferSize = 1024;
        public event EventHandler<string> NetworkCommandReceived;
        public event EventHandler<string> ErrorOccurred;
        public event EventHandler<string> ServicesMessagesClient;
        public event PropertyChangedEventHandler PropertyChanged;


        private string clientMessages;
        public string ClientMessages
        {
            get { return clientMessages; }
            set
            {
                if (value != clientMessages)
                {
                    clientMessages = value;
                    OnPropertyChanged(nameof(ClientMessages));
                }
            }
        }


        private string networkCommand;
        public string NetworkCommand
        {
            get { return networkCommand; }
            set
            {
                if (value != networkCommand)
                {
                    networkCommand = value;
                    OnPropertyChanged(nameof(NetworkCommand));
                }
            }
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                if (value != errorMessage)
                {
                    errorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }
        }


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


        public async Task<object> SendCommandAndGetGameAsync(string command)
        {
            try
            {
                if (!IsConnected())
                {
                    ClientServicesMessages("Команда не отправлена, нет соединения с сервером");
                    return null;
                }

                byte[] commandBytes = Encoding.Unicode.GetBytes(command);
                await SendDataAsync(commandBytes); // Отправка команды асинхронно
                ClientServicesMessages("Команда отправлена: " + command);

                byte[] receiveBuffer = new byte[bufferSize];
                int bytesRead = await ReceiveDataAsync(receiveBuffer); // Получение ответа асинхронно

                // Определяем тип принятых данных
                string receivedData = Encoding.Unicode.GetString(receiveBuffer, 0, bytesRead);
                ClientServicesMessages("Принят ответ: " + command);

                if (receivedData.StartsWith("gamedata"))
                {
                    // Если принятые данные представляют объект Game
                    byte[] gameData = new byte[bytesRead - 8]; // 8 байт - длина маркера "gamedata"
                    Array.Copy(receiveBuffer, 8, gameData, 0, bytesRead - 8);
                    return Game.DeserializeGame(gameData);
                }
                else if (receivedData.StartsWith("textdata"))
                {
                    // Если принятые данные являются текстовой строкой
                    return receivedData.Substring(8); // Обрезаем маркер "textdata"
                }
                else
                {
                    // Неизвестный тип данных
                    return "Неизвестный тип данных";
                }

            }
            catch (Exception ex)
            {
                // Обработка ошибок
                //Console.WriteLine("Ошибка при отправке команды и получении экземпляра игры: " + ex.Message);
                HandleError("Ошибка при отправке команды и получении экземпляра игры: " + ex.Message);
                return null;
            }
        }

        private Task SendDataAsync(byte[] data)
        {
            return Task.Factory.FromAsync(clientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, null, null),
                clientSocket.EndSend);
        }

        private async Task<int> ReceiveDataAsync(byte[] buffer)
        {
            return await Task<int>.Factory.FromAsync(clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, null, null),
                clientSocket.EndReceive);
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




        // Метод для обработки сообщений сервера
        public void ClientServicesMessages(string messages)
        {
            ClientMessages = messages;
            ServicesMessagesClient?.Invoke(this, messages);
        }



        // Метод для обработки сетевых команд
        public void ReceiveNetworkCommand(string command)
        {
            NetworkCommand = command;
            NetworkCommandReceived?.Invoke(this, command);
        }

        // Метод для обработки ошибок
        public void HandleError(string errorMessage)
        {
            ErrorMessage = errorMessage;
            ErrorOccurred?.Invoke(this, errorMessage);
        }



        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
