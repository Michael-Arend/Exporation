using Poker.Infrastructure.Enums;
using Poker.Infrastructure.Models;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;

namespace Poker.Infrastructure.HistoryBuilder
{
    public class HistoryBuilder
    {
        private StringBuilder builder;
        private string fileName;

        public HistoryBuilder()
        {
            this.builder = new StringBuilder();
        }


        public void Reset()
        {
            builder = new StringBuilder();
        }

        public void BuildHeader(Round round)
        {
            var rdm = new Random();
            var handString = "";
            for (var i = 0; i < 13; i++)
            {
                handString += rdm.Next(0, 9).ToString();
            }

            var time = DateTime.Now;
            builder.AppendLine(
                $"PokerStars Zoom Hand #{handString}:  Hold'em No Limit (${(round.BigBlind / 2).ToString("0.00").Replace(",", ".")}/${(round.BigBlind).ToString("0.00").Replace(",", ".")}) - {time.Year}/{time.Month}/{time.Day} {time.Hour}:{time.Minute.ToString("00")}:{time.Second.ToString("00")} ET");
            builder.AppendLine("Table 'Halley' 6-max Seat #6 is the button");


            var seat = 1;
            foreach (var player in round.Players)
            {
                builder.AppendLine($"Seat {seat}: {player.Name} (${(player.Chips * round.BigBlind).ToString("0.00").Replace(",", ".")} in chips)");
                seat++;
            }

            var bb = round.Players.FirstOrDefault(x => x.Position == Position.BB);
            var sb = round.Players.FirstOrDefault(x => x.Position == Position.SB);
            var hero = round.Players.FirstOrDefault(x => x.Name == "HeroGto");
            var heroCards = hero.Hand.GetStringFromHand().Insert(2, " ");
            builder.AppendLine($"{sb.Name}: posts small blind ${(round.SmallBlind).ToString("0.00").Replace(",", ".")}");
            builder.AppendLine($"{bb.Name}: posts big blind ${round.BigBlind.ToString("0.00").Replace(",", ".")}");
            builder.AppendLine("*** HOLE CARDS ***");
            builder.AppendLine($"Dealt to {hero.Name} [{heroCards}]");
        }

        public void SaveHistoryToFile(string location)
        {
            var contents = "";
            string path = @"C:\PioSolver\hh.txt"; // path to file
            if (File.Exists(path))
            {
                contents = File.ReadAllText(path);

            }

            using (FileStream fs = File.Create(path))
            {
                // writing data in string
                if (contents.Length > 0)
                {
                    builder.AppendLine();
                    builder.AppendLine();
                }
                byte[] info = new UTF8Encoding(true).GetBytes(contents + builder.ToString());
                fs.Write(info, 0, info.Length);

                // writing data in bytes already
                byte[] data = new byte[] { 0x0 };
                fs.Write(data, 0, data.Length);
            }
        }

        public void PlayerRaises(Player player, decimal difference, decimal overall, Round round)
        {
           
            var overallString = (overall * round.BigBlind).ToString("0.00").Replace(",", ".");
            var allinString = player.Chips == 0 ? "and is all-in" : "";
            var differenceString = (difference  * round.BigBlind).ToString("0.00").Replace(",", ".");
            builder.AppendLine($"{player.Name}: raises ${differenceString} to ${overallString} {allinString}");
        }

        public void PlayerBets(Player player, decimal size, Round round)
        {
            var overallString = (size * round.BigBlind ).ToString("0.00").Replace(",", ".");
            var allinString = player.Chips == 0 ? "and is all-in" : "";
            builder.AppendLine($"{player.Name}: bets ${overallString} {allinString}");
        }

        public void PlayerCalls(Player player, decimal toCallAmount, Round round)
        {
            var overallString = (toCallAmount * round.BigBlind ).ToString("0.00").Replace(",", ".");
            builder.AppendLine($"{player.Name}: calls ${overallString}");
        }

        public void PlayerCallsAllin(Player player, decimal toCallAmount, decimal chips, Round round)
        {
            var chipsString = (chips * round.BigBlind).ToString("0.00").Replace(",", ".");
            builder.AppendLine($"{player.Name}: calls ${chipsString} and is all-in");
            if(chips < toCallAmount)
            {
                var returnString = (toCallAmount - chips * round.BigBlind).ToString("0.00").Replace(",", ".");
                builder.AppendLine($"Uncalled bet (${returnString}) returned to {round.PlayersInHand.First(x=>x.Name != player.Name).Name}");
            }
        }

        public void PlayerFolds(Player player)
        {
            builder.AppendLine($"{player.Name}: folds");
        }




        public void PlayerChecks(Player player)
        {
            builder.AppendLine($"{player.Name}: checks");
        }

        public void HandleWin(Player player, decimal returnAmount, Round round, Boolean withShowDown)
        {
            if (returnAmount > 0)
            {
                var returnString = (returnAmount * round.BigBlind).ToString("0.00").Replace(",", ".");
                builder.AppendLine($"Uncalled bet (${returnString}) returned to {player.Name}");
            }
            
            var potString = ((round.Pot * round.BigBlind - returnAmount* round.BigBlind)).ToString("0.00").Replace(",", ".");

            if (withShowDown)
            {
                HandleWinShowdown(round.PlayersInHand.First(), round.PlayersInHand.Last(), round.Board, round);
            }
            else
            {
                builder.AppendLine($"{player.Name} collected ${potString} from pot");
                builder.AppendLine($"{player.Name}: doesn´t show hand");
            }

            builder.AppendLine("*** SUMMARY ***"); 
            builder.AppendLine($"Total pot ${potString} | Rake $0 ");
            if(round.Board.Any())
            {
                var boardString = "";
                foreach (var card in round.Board)
                {
                    boardString += " ";
                    boardString += card.GetStringFromCard();
                }
                builder.AppendLine($"Board [{boardString.Substring(1)}]");
            }
            var seat = 1;
            foreach (var p in round.Players)
            {
                var posString = p.Position == Position.BTN ? "(button) " :
                    p.Position == Position.SB ? "(small blind) " :
                    p.Position == Position.BB ? "(big blind) " : "";

                var didNotBet = p.MaxStreetReached == Street.Preflop && p.ChipsInvestedInRound == 0
                    ? " (didn´t bet)"
                    : "";

                if (!p.PlayerInHand)
                    builder.AppendLine($"Seat {seat}: {p.Name} {posString}folded {StreetToString(p.MaxStreetReached)}{didNotBet}");
                else if (p.Name == player.Name)
                {
                    if(withShowDown)
                    {
                        var playerHand = round.Board.ToList();
                        playerHand.AddRange(new List<Card> { p.Hand.Card1, p.Hand.Card2 });
                        var resultPlayer1 = new Result(playerHand);
                        builder.AppendLine($"Seat {seat}: {p.Name} {posString} showed [{p.Hand.GetStringFromHand()}] and won (${potString}) with {resultPlayer1.Message}");
                    }
                    else
                    {
                        builder.AppendLine($"Seat {seat}: {player.Name} {posString}collected ${potString}");
                }
                }
                else if (p.Name != player.Name)
                    {
                    var playerHand = round.Board.ToList();
                    playerHand.AddRange(new List<Card> { p.Hand.Card1, p.Hand.Card2 });
                    var resultPlayer1 = new Result(playerHand);
                    builder.AppendLine($"Seat {seat}: {p.Name} {posString} showed [{p.Hand.GetStringFromHand()}] and lost with {resultPlayer1.Message}");
                }
                seat++;
            }
            builder.AppendLine();
        }

        private void HandleWinShowdown(Player playerOne, Player playerTwo, List<Card> board, Round round) 
        {
            var playerOneHand = board.ToList();
            playerOneHand.AddRange(new List<Card> { playerOne.Hand.Card1, playerOne.Hand.Card2 });
            var resultPlayer1 = new Result(playerOneHand);
            var playerTwoHand = board.ToList();
            playerTwoHand.AddRange(new List<Card> { playerTwo.Hand.Card1, playerTwo.Hand.Card2 });
            var resultPlayer2 = new Result(playerTwoHand);
            builder.AppendLine("*** SHOW DOWN ***");
            builder.AppendLine($"{playerOne.Name}: shows [{playerOne.Hand.GetStringFromHand()}] ({resultPlayer1.Message})");
            builder.AppendLine($"{playerTwo.Name}: shows [{playerTwo.Hand.GetStringFromHand()}] ({resultPlayer2.Message})");

            if(resultPlayer1.Rating > resultPlayer2.Rating)
            {
                var potString = (round.Pot * round.BigBlind).ToString("0.00").Replace(",", ".");
                builder.AppendLine($"{playerOne.Name} collected (${potString}) from the pot");
            }
            if (resultPlayer1.Rating < resultPlayer2.Rating)
            {
                var potString = (round.Pot * round.BigBlind).ToString("0.00").Replace(",", ".");
                builder.AppendLine($"{playerTwo.Name} collected (${potString}) from the pot");
            }
            if (resultPlayer1.Rating == resultPlayer2.Rating)
            {
                var potString = (round.Pot/2 * round.BigBlind).ToString("0.00").Replace(",", ".");
                builder.AppendLine($"{playerOne.Name} collected (${potString}) from the pot");
                builder.AppendLine($"{playerTwo.Name} collected (${potString}) from the pot");
            }
        }


        public string StreetToString(Street street)
        {
            switch (street)
            {
                case Street.Preflop:
                    return "before the Flop";
                case Street.Flop:
                    return "on the Flop";
                case Street.Turn:
                    return "on the Turn";
                case Street.River:
                    return "on the River";
                case Street.Showdown:
                    return "No Fold";
            }
            return "";
        }

        internal void HandleNewStreet(string street, List<Card> board)
        {
            var board1 = $"{board[0].GetStringFromCard()} {board[1].GetStringFromCard()} {board[2].GetStringFromCard()}"; 
           
            if (board.Count == 5) { board1 += $" {board[board.Count - 2].GetStringFromCard()}"; }

            var board2 = board.Count == 3 ? "" : $"[{board.Last().GetStringFromCard()}]" ;

                builder.AppendLine($"*** {street.ToUpper()} *** [{board1}] {board2}");
           
        }

     
    }
}
