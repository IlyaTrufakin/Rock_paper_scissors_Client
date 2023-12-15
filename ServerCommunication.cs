﻿using System;
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
        public event EventHandler<string> ServicesMessagesServer;
        public event PropertyChangedEventHandler PropertyChanged;


        private string serverMessages;
        public string ServerMessages
        {
            get { return serverMessages; }
            set
            {
                if (value != serverMessages)
                {
                    serverMessages = value;
                    OnPropertyChanged(nameof(ServerMessages));
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


        public ServerCommunication(string ipAddress = "127.0.0.1", int port = 8005)
        {
            try
            {
                ipPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (FormatException ex)
            {
                HandleError("Ошибка IP-адреса: " + ex.Message);
            }
            catch (ArgumentException ex)
            {
                HandleError("Ошибка в аргументах: " + ex.Message);
            }
            catch (Exception ex)
            {
                HandleError("Произошла ошибка: " + ex.Message);
            }
        }



        public async void Start()
        {
            try
            {
                listenSocket.Bind(ipPoint);
                listenSocket.Listen(10);
                ServerServicesMessages("Server start listen...");

                while (true)
                {
                    Socket handler = await listenSocket.AcceptAsync();
                    AddClient(handler);

                    ServerServicesMessages($"Client connected:  {connectedClients[handler]} IP({handler.RemoteEndPoint})");
                    ServerServicesMessages($"\tList of connected clients: ");
                    foreach (var clients in connectedClients)
                    {
                        ServerServicesMessages($"\t- {clients.Value} IP({clients.Key.RemoteEndPoint})");
                    }

                    ThreadPool.QueueUserWorkItem(new WaitCallback(HandleClient), handler);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex.Message);
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


        // private string Server


        private void HandleClient(object obj)
        {
            Socket handler = (Socket)obj;
            ServerServicesMessages("Соккет клиента получен");
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

                    //string[] StringParts = receivedString.ToString().Split(':');
                    string StringParts = receivedString.ToString();

                    if (StringParts != "timeQuiet") // когда клиент запрашивает время в автоматическом режиме, не выводим об этом инфо в консоль
                    {
                        ServerServicesMessages(DateTime.Now.ToShortTimeString() + ": " + StringParts + $"  (from {connectedClients[handler]})");
                    }


                    ReceiveNetworkCommand(StringParts);
                    string response = ProcessRequest(StringParts); // обработка строки запроса от клиента

                    handler.Send(Encoding.Unicode.GetBytes(response)); // отправка ответа клиенту

                    if (response == "Closing" || StringParts == "maxсonnectionlimit") // отработка запроса клиента на закрытие соединения
                    {
                        ServerServicesMessages($"{connectedClients[handler]} - Closing connection...");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(ex.Message);
            }
            finally
            {
                connectedClients.Remove(handler);
                handler.Shutdown(SocketShutdown.Both);
                ServerServicesMessages("Connection closed");
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

                case "paper":
                    response = ($"paper");
                    break;

                case "reject":
                    response = ($"Идентификация клиента не осуществлена.");
                    break;

                case "maxсonnectionlimit":
                    response = ($"Сервер перегружен. Попробуйте присоединиться позже.");
                    break;


                case "newgame":
                    response = ($"newgame");
                    break;

                case "info":
                    response = Environment.OSVersion.ToString();
                    break;

                case "get":
                    response = Environment.OSVersion.ToString();
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



        // Метод для обработки сообщений сервера
        public void ServerServicesMessages(string messages)
        {
            ServerMessages = messages;
            ServicesMessagesServer?.Invoke(this, messages);
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