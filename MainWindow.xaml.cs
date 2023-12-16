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
                ScrollTextBlock.ScrollToEnd();
                if (clientCommunication.IsConnected())
                {
                    try
                    {
                        object response = await clientCommunication.SendCommandAndGetGameAsync($"newgame");
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







        private void IsServerCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (IsServerCheckBox.IsChecked == true)
            {
                ConnectButton.Content = "Start server";
                PaperButton.IsEnabled = false;
                ScissorsButton.IsEnabled = false;
                RockButton.IsEnabled = false;
                RandomStepButton.IsEnabled = false;
                I_give_upButton.IsEnabled = false;
                StandoffButton.IsEnabled = false;
            }
            else
            {
                ConnectButton.Content = "Connect to server";
                PaperButton.IsEnabled = true;
                ScissorsButton.IsEnabled = true;
                RockButton.IsEnabled = true;
                RandomStepButton.IsEnabled = true;
                I_give_upButton.IsEnabled = true;
                StandoffButton.IsEnabled = true;
            }

        }



        // Обработка сетевых команд от сервера
        private void ServerCommunication_NetworkCommandReceived(object sender, string command)
        {
            Dispatcher.Invoke(() =>
            {
                OutputWindow.Text += "Команда серверу: " + command + Environment.NewLine;
                GameHandler(command);
                ScrollTextBlock.ScrollToEnd();
            });

        }

        // Обработка сообщений ошибок сервера
        private void ServerCommunication_ErrorOccurred(object sender, string errorMessages)
        {
            Dispatcher.Invoke(() =>
            {
                OutputWindow.Text += "Ошибка сервера: " + errorMessages + Environment.NewLine;
                ScrollTextBlock.ScrollToEnd();
            });

        }



        // Обработка служебных сообщений от сервера
        private void ServerCommunication_ServicesMessagesServer(object sender, string command)
        {
            Dispatcher.Invoke(() =>
            {
                OutputWindow.Text += "Сообщение сервера: " + command + Environment.NewLine;
                ScrollTextBlock.ScrollToEnd();
            });

        }


        private void GameHandler(string command)
        {
            var signKey = game.Sign.FirstOrDefault(x => x.Value == command).Key;

            if (signKey != 0) // Проверяем, было ли найдено соответствие
            {
                OutputWindow.Text += "результат раунда: " + game.Play(signKey) + Environment.NewLine;
                VyctoryTextBlock.Text = game.Victory.ToString();
                DrawTextBlock.Text = game.Score_Draw.ToString();
                DefeatTextBlock.Text = game.Defeats.ToString();
                GameRoundTextBlock.Text = game.Round.ToString() + " РАУНД";
            }
            else
            {
                OutputWindow.Text += "Неверный выбор: " + command + Environment.NewLine;
            }
            ScrollTextBlock.ScrollToEnd();

        }

        private async void ScissorsButton_Click(object sender, RoutedEventArgs e)
        {
            if (clientCommunication.IsConnected())
            {

                try
                {
                    object response = await clientCommunication.SendCommandAndGetGameAsync("scissors");
                    OutputWindow.Text += "Ответ сервера: " + response + Environment.NewLine;
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

        private async void RockButton_Click(object sender, RoutedEventArgs e)
        {
            if (clientCommunication.IsConnected())
            {

                try
                {
                    object response = await clientCommunication.SendCommandAndGetGameAsync("rock");
                    OutputWindow.Text += "Ответ сервера: " + response + Environment.NewLine;
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


        private async void PaperButton_Click(object sender, RoutedEventArgs e)
        {
            if (clientCommunication.IsConnected())
            {

                try
                {
                    object response = await clientCommunication.SendCommandAndGetGameAsync("paper");
                    OutputWindow.Text += "Ответ сервера: " + response + Environment.NewLine;
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
    }
}