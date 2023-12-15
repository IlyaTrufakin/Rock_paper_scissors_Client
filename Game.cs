using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock_paper_scissors_Client
{

 
    internal class Game
    {
        public int Round;
        public int Victory;
        public int Defeats;
        public int Score_Draw;
        public Dictionary<int, string> Sign;
        private Random random = new Random();
        public Game() 
        {
            Round = 0;  
            Victory = 0;
            Defeats = 0;
            Score_Draw = 0;
            Sign = new Dictionary<int, string>
            {
                { 1, "rock" },
                { 2, "scissors" },
                { 3, "paper" }
            };
        }

        public int GetRandomSign()
        {
            return random.Next(Sign.Count)+1;
        }

        public string PlayRound(int playerSign, int computerSign = 0)
        {
            if (computerSign == 0)
            {
            computerSign = GetRandomSign();
            }

            string playerChoice = Sign.ContainsKey(playerSign) ? Sign[playerSign] : "Invalid choice";
            string computerChoice = Sign.ContainsKey(computerSign) ? Sign[computerSign] : "Invalid choice";

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


    }
}
