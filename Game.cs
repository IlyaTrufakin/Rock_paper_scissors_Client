using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rock_paper_scissors_Client
{

    [Serializable]
    public class Game
    {
        public int Round;
        public int Victory;
        public int Defeats;
        public int Score_Draw;
        public int Plays;
        public int PlayerSign;
        public int ServerSign;
        public Dictionary<int, string> Sign;
        private Random random = new Random();
        private const int playsCount = 5;

        public Game()
        {
            Round = 1;
            Victory = 0;
            Defeats = 0;
            Score_Draw = 0;
            Plays = 0;
            Sign = new Dictionary<int, string>
            {
                { 1, "rock" },
                { 2, "scissors" },
                { 3, "paper" }
            };
            PlayerSign = 0;
            ServerSign = 0;
        }

        public int GetRandomSign()
        {
            return random.Next(Sign.Count) + 1;
        }

        public string Play(int playerSign, int computerSign = 0)
        {
            if (computerSign == 0)
            {
                computerSign = GetRandomSign();
             }

            string playerChoice = Sign.ContainsKey(playerSign) ? Sign[playerSign] : "Invalid choice";
            string computerChoice = Sign.ContainsKey(computerSign) ? Sign[computerSign] : "Invalid choice";
            ServerSign = computerSign;
            PlayerSign = playerSign;

            if (Plays >= playsCount)
            {
                Round++;
                Plays = 0;
            }


            if (playerChoice == computerChoice)
            {
                Score_Draw++;
                return "Draw";
            }
            else if ((playerChoice == "rock" && computerChoice == "scissors") ||
                     (playerChoice == "scissors" && computerChoice == "paper") ||
                     (playerChoice == "paper" && computerChoice == "rock"))
            {
                Victory++;
                return "You win!";
            }
            else
            {
                Defeats++;
                return "You lose!";
            }
        }


        public byte[] SerializeGame()
        {
            string jsonString = JsonSerializer.Serialize(this); // сериализуем текущий объект Game в формат JSON
            return Encoding.UTF8.GetBytes(jsonString); // возвращаем сериализованные данные в виде массива байтов
        }

        public static Game DeserializeGame(byte[] data)
        {
            string jsonString = Encoding.UTF8.GetString(data); // преобразуем массив байтов в строку
            return JsonSerializer.Deserialize<Game>(jsonString); // десериализуем объект Game из строки JSON
        }


    }
}
