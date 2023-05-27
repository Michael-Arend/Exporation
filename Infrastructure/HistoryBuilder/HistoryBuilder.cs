using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Poker.Infrastructure.Models;

namespace Poker.Infrastructure.HistoryBuilder
{
    public class HistoryBuilder
    {
        private StringBuilder builder;
        private string fileName;

        public  HistoryBuilder()
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
                $"PokerStars Zoom Hand #{handString}:  Hold'em No Limit (${(round.BigBlind/2).ToString("0.00").Replace(",", ".")}/${(round.BigBlind).ToString("0.00").Replace(",", ".")}) - {time.Year}/{time.Month}/{time.Day} {time.Hour}:{time.Minute.ToString("00")}:{time.Second.ToString("00")} ET");
            builder.AppendLine("Table 'Halley' 6-max Seat #6 is the button");

         
            var seat = 1;
            foreach (var player in round.Players)
            {
                builder.AppendLine($"Seat {seat}: {player.Name}(${player.Chips * round.Limit} in chips)");
                seat++;
            }

            var bb = round.Players.FirstOrDefault(x => x.Position == Position.BB);
            var sb = round.Players.FirstOrDefault(x => x.Position == Position.SB);
            var hero = round.Players.FirstOrDefault(x => x.Name == "HeroGto");
            var heroCards = hero.Hand.GetStringFromHand().Insert(2," ");
            builder.AppendLine($"{sb.Name}: posts small blind ${(round.Limit / 2).ToString(CultureInfo.InvariantCulture)}");
            builder.AppendLine($"{bb.Name}: posts big blind ${round.Limit.ToString(CultureInfo.InvariantCulture)}");
            builder.AppendLine("***HOLE CARDS***");
            builder.AppendLine($"Dealt to {hero.Name} [{heroCards}]");
            }

        public void SaveHistoryToFile(string location)
        {

            string path = @"C:\temp\hh.txt"; // path to file
            string contents = File.ReadAllText(path);

            using (FileStream fs = File.Create(path))
            {
                // writing data in string
                if (contents.Length > 0)
                {
                    builder.AppendLine();
                    builder.AppendLine();
                }
                byte[] info = new UTF8Encoding(true).GetBytes(builder.ToString()+ contents);
                fs.Write(info, 0, info.Length);

                // writing data in bytes already
                byte[] data = new byte[] { 0x0 };
                fs.Write(data, 0, data.Length);
            }
        }

        public void PlayerRaises(Player player, decimal difference, decimal overall, Round round)
        {
            var overallString = (overall*round.BigBlind * round.Limit).ToString("0.00").Replace(",",".");
            var differenceString = (difference * round.BigBlind * round.Limit).ToString("0.00").Replace(",", ".");
            builder.AppendLine($"{player.Name}: raises ${differenceString} to ${overallString}");
        }

        public void PlayerCalls(Player player, decimal toCallAmount,Round round)
        {
            var overallString = (toCallAmount * round.BigBlind * round.Limit).ToString("0.00").Replace(",", ".");

            builder.AppendLine($"{player.Name}: calls ${overallString}");
        }

        public void PlayerFolds(Player player)
        {
            builder.AppendLine($"{player.Name}: folds");
        }

        public void HandleWinNoShowdown(Player player, decimal returnAmount, Round round)
        {
            if (returnAmount > 0)
            {
                var returnString = (returnAmount * round.BigBlind * round.Limit).ToString("0.00").Replace(",", ".");
                builder.AppendLine($"Uncalled bet (${returnString}) returned to {player.Name}");
            }

            var potString = ((round.Pot- returnAmount) * round.BigBlind * round.Limit).ToString("0.00").Replace(",", ".");

               
                builder.AppendLine($"{player.Name} collected ${potString} from pot");
                builder.AppendLine($"{player.Name} doesn´t show hand");
                builder.AppendLine("*** SUMMARY ***");
                builder.AppendLine($"Total pot ${potString} | Rake $0 ");

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
                        builder.AppendLine($"Seat {seat}: {player.Name} {posString}collected ${potString}");
                    seat++;
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
      

    }
}
