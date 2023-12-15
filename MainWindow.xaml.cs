using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Rock_paper_scissors_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private ClientCommunication clientCommunication;
        private ServerCommunication server;
        private Game game;

        public MainWindow()
        {
            InitializeComponent();

            // Создаем таймер и устанавливаем интервал на 1 секунду
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsServerCheckBox.IsChecked == true)
            {
                try
                {
                    server = new ServerCommunication("127.0.0.1", 8005);
                    server.NetworkCommandReceived += ServerCommunication_NetworkCommandReceived;
                    server.ErrorOccurred += ServerCommunication_ErrorOccurred;
                    server.ServicesMessagesServer += ServerCommunication_ServicesMessagesServer;
                    server.Start();
                    game = new Game();
                }
                catch (Exception ex)
                {
                    OutputWindow.Text += "Произошла ошибка: " + ex.Message + Environment.NewLine;
                }
            }
            else
            {
                clientCommunication = new ClientCommunication();
                string connectionResult = clientCommunication.ConnectToServer(ipAddress.Text, portNumber.Text);
                OutputWindow.Text += connectionResult + Environment.NewLine;
                SendMessageButton.IsEnabled = clientCommunication.IsConnected();
                ScrollTextBlock.ScrollToEnd();
                if (clientCommunication.IsConnected())
                {
                    try
                    {
                        string response = await clientCommunication.SendMessageAndReceiveResponseAsync($"newgame");
                        OutputWindow.Text += "Ответ сервера: " + response + Environment.NewLine;
                        game = new Game();
                    }
                    catch (Exception ex)
                    {
                        OutputWindow.Text += "Ошибка: " + ex.Message + Environment.NewLine;
                    }

                }
                else
                {
                    OutputWindow.Text += "Сообщение не отправлено: нет соединения с сервером" + Environment.NewLine;
                }
            }
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SendMessageButton.IsEnabled = false;
        }

        private async void Timer_Tick(object sender, EventArgs e) // получаем время с серврера каждую секунду
        {
            /*          if (clientCommunication.IsConnected())
                      {
                          try
                          {
                              string response = await clientCommunication.SendMessageAndReceiveResponseAsync("timeQuiet");
                              Status1.Text = "Время Сервера: " + response;
                              Status2.Text = "Соединение с сервером: установлено";
                          }
                          catch (Exception ex)
                          {
                              OutputWindow.Text += "Ошибка: " + ex.Message + Environment.NewLine;
                          }

                      }
                      else
                      {
                          Status1.Text = "Время Сервера: не доступно";
                          Status2.Text = "Соединение с сервером: не установлено";
                      }*/
        }



        private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (clientCommunication.IsConnected())
            {
                if (InputWindow.Text.Length > 0)
                {
                    try
                    {
                        string response = await clientCommunication.SendMessageAndReceiveResponseAsync(InputWindow.Text);
                        OutputWindow.Text += "Ответ сервера: " + response + Environment.NewLine;
                    }
                    catch (Exception ex)
                    {
                        OutputWindow.Text += "Ошибка: " + ex.Message + Environment.NewLine;
                    }
                }
            }
            else
            {
                OutputWindow.Text += "Сообщение не отправлено: нет соединения с сервером" + Environment.NewLine;
            }
            ScrollTextBlock.ScrollToEnd();
        }





        private async void InputWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (clientCommunication.IsConnected())
                {
                    if (InputWindow.Text.Length > 0)
                    {
                        try
                        {
                            string response = await clientCommunication.SendMessageAndReceiveResponseAsync(InputWindow.Text);
                            OutputWindow.Text += "Ответ сервера: " + response + Environment.NewLine;
                        }
                        catch (Exception ex)
                        {
                            OutputWindow.Text += "Ошибка: " + ex.Message + Environment.NewLine;
                        }
                    }

                }
                else
                {
                    OutputWindow.Text += "Сообщение не отправлено: нет соединения с сервером" + Environment.NewLine;
                }
                ScrollTextBlock.ScrollToEnd();
            }
        }



        private void IsServerCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (IsServerCheckBox.IsChecked == true)
            {
                ConnectButton.Content = "Start server";
            }
            else
            {
                ConnectButton.Content = "Connect to server";
            }

        }

        private async void PaperButton_Click(object sender, RoutedEventArgs e)
        {
            if (clientCommunication.IsConnected())
            {

                try
                {
                    string response = await clientCommunication.SendMessageAndReceiveResponseAsync("paper");
                    OutputWindow.Text += "Ответ сервера: " + response + Environment.NewLine;
                    OutputWindow.Text += "результат раунда: " + game.PlayRound(1) + Environment.NewLine;
                }
                catch (Exception ex)
                {
                    OutputWindow.Text += "Ошибка: " + ex.Message + Environment.NewLine;
                }

            }
            else
            {
                OutputWindow.Text += "Сообщение не отправлено: нет соединения с сервером" + Environment.NewLine;
            }
            ScrollTextBlock.ScrollToEnd();
        }


        // Обработка сетевых команд от сервера
        private void ServerCommunication_NetworkCommandReceived(object sender, string command)
        {
            Dispatcher.Invoke(() =>
            {
                OutputWindow.Text += "Команда серверу: " + command + Environment.NewLine;
            });
        }

        // Обработка сообщений ошибок сервера
        private void ServerCommunication_ErrorOccurred(object sender, string errorMessages)
        {
            Dispatcher.Invoke(() =>
            {
                OutputWindow.Text += "Ошибка сервера: " + errorMessages + Environment.NewLine;
            });
        }



        // Обработка служебных сообщений от сервера
        private void ServerCommunication_ServicesMessagesServer(object sender, string command)
        {
            Dispatcher.Invoke(() =>
            {
                OutputWindow.Text += "Сообщение сервера: " + command + Environment.NewLine;
            });
        }

    }
}