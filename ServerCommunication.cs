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
    internal class ServerCommunication : INotifyPropertyChanged
    {
        private readonly int maxClients = 3; // макс. кол-во одновременных клиентов на соккете
        IPEndPoint ipPoint;
        private Socket listenSocket;
        private Dictionary<Socket, string> connectedClients = new Dictionary<Socket, string>();
        private int clientIdCounter = 0;
        private object lockObject = new object(); // для блокировки доступа нескольких потоков к изменяемому объекту
        public event EventHandler<string> NetworkCommandReceived;
        public event EventHandler<string> ErrorOccurred;

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


        public ServerCommunication(string ipAddress = "127.0.0.1", int port = 8005)
        {
            try
            {
                ipPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (FormatException ex)
            {
                //Console.WriteLine("Ошибка IP-адреса: " + ex.Message);
            }
            catch (ArgumentException ex)
            {
                //Console.WriteLine("Ошибка в аргументах: " + ex.Message);
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Произошла ошибка: " + ex.Message);
            }
        }



        public async void Start()
        {
            try
            {
                listenSocket.Bind(ipPoint);
                listenSocket.Listen(10);
                //Console.WriteLine("Server start listen...");

                while (true)
                {
                    Socket handler =await listenSocket.AcceptAsync();
                    AddClient(handler);

                    //Console.WriteLine($"Client connected:  {connectedClients[handler]} IP({handler.RemoteEndPoint})");
                    //Console.WriteLine($"\tList of connected clients: ");
                    foreach (var clients in connectedClients)
                    {
                        //Console.WriteLine($"\t- {clients.Value} IP({clients.Key.RemoteEndPoint})");
                    }

                    ThreadPool.QueueUserWorkItem(new WaitCallback(HandleClient), handler);
                    //Console.WriteLine("Создан поток: " + clientThread.GetHashCode());

                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
        }



        // Добавление клиента и его идентификатора в словарь connectedClients
        private void AddClient(Socket clientSocket)
        {
            lock (lockObject)
            {
                clientIdCounter++;
                connectedClients.Add(clientSocket, "Client ID#" + clientIdCounter.ToString());
            }
        }





        private void HandleClient(object obj)
        {
            Socket handler = (Socket)obj;
           
            try
            {

                while (true)
                {
                    byte[] data = new byte[256];
                    StringBuilder receivedString = new StringBuilder();
                    int receivedBytes;

                    do
                    {
                        receivedBytes = handler.Receive(data);
                        receivedString.Append(Encoding.Unicode.GetString(data, 0, receivedBytes));
                    } while (handler.Available > 0);

                    string[] StringParts = receivedString.ToString().Split(':');

                    if (StringParts[0] != "timeQuiet") // когда клиент запрашивает время в автоматическом режиме, не выводим об этом инфо в консоль
                    {
                        //Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + StringParts[0] + $"  (from {connectedClients[handler]})");
                    }


                    if (StringParts[0] == "login") // когда клиент подключается и присылает логин и пароль
                    {
                        //Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + StringParts[0] + $"  (from {connectedClients[handler]})" + $"Идентификация клиента: {StringParts[0]} : {StringParts[1]} : {StringParts[2]}");

                        if (connectedClients.Count > maxClients)
                        {
                            StringParts[0] = "maxсonnectionlimit";
                        }

                    }

                    string response = ProcessRequest(StringParts[0]); // обработка строки запроса от клиента

                    handler.Send(Encoding.Unicode.GetBytes(response)); // отправка ответа клиенту

                    if (response == "Closing" || StringParts[0] == "reject" || StringParts[0] == "maxсonnectionlimit") // отработка запроса клиента на закрытие соединения
                    {
                        //Console.Write($"{connectedClients[handler]} - Closing connection...");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
            finally
            {
                connectedClients.Remove(handler);
                handler.Shutdown(SocketShutdown.Both);
                //Console.WriteLine("Connection closed");
                handler.Close();

            }



        }


        private string ProcessRequest(string request)
        {
            string response = string.Empty;

            switch (request.Trim().ToLower())
            {

                case "time":
                    response = DateTime.Now.ToString();
                    break;

                case "login":
                    response = ($"Идентификация клиента осуществлена: {request}");
                    break;

                case "reject":
                    response = ($"Идентификация клиента не осуществлена.");
                    break;

                case "maxсonnectionlimit":
                    response = ($"Сервер перегружен. Попробуйте присоединиться позже.");
                    break;


                case "timequiet":
                    response = DateTime.Now.ToString();
                    break;

                case "info":
                    response = Environment.OSVersion.ToString();
                    break;

                case "get":
                    //Console.WriteLine("Запрошен ручной ответ, напишите что-нибудь клиенту: ");
                    response = Environment.OSVersion.ToString();
                    //response = Console.ReadLine();
                    break;

                case "bye":
                    response = "Closing";
                    break;

                default:
                    response = "Invalid command";
                    break;
            }

            return response;
        }


   

        // Метод для обработки сетевых команд
        public void ReceiveNetworkCommand(string command)
        {
            NetworkCommand = command;
            NetworkCommandReceived?.Invoke(this, command);
        }

        // Метод для обработки ошибок
        public void HandleError(Exception ex)
        {
            ErrorMessage = ex.Message;
            ErrorOccurred?.Invoke(this, ex.Message);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
