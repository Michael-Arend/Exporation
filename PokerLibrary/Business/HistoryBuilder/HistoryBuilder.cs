using System.Text;
using PokerLibrary.Infrastructure.Enums;
using PokerLibrary.Infrastructure.Models;

namespace PokerLibrary.Business.HistoryBuilder;

public class HistoryBuilder
{
    private StringBuilder _builder;

    public HistoryBuilder()
    {
        _builder = new StringBuilder();
    }


    public void Reset()
    {
        _builder = new StringBuilder();
    }

    public void BuildHeader(Round round)
    {
        var rdm = new Random();
        var handString = rdm.Next(0, 10000000).ToString("D7");
        handString += rdm.Next(0, 1000000).ToString("D6");

        var time = DateTime.Now;
        _builder.AppendLine(
            $"PokerStars Zoom Hand #{handString}:  Hold'em No Limit (${(round.BigBlind / 2).ToString("0.00").Replace(",", ".")}/${round.BigBlind.ToString("0.00").Replace(",", ".")}) - {time.Year}/{time.Month}/{time.Day} {time.Hour}:{time.Minute.ToString("00")}:{time.Second.ToString("00")} ET");
        _builder.AppendLine("Table 'Halley' 6-max Seat #6 is the button");


        var seat = 1;
        foreach (var player in round.Players)
        {
            _builder.AppendLine(
                $"Seat {seat}: {player.Name} (${(player.Chips * round.BigBlind).ToString("0.00").Replace(",", ".")} in chips)");
            seat++;
        }

        var bb = round.Players.FirstOrDefault(x => x.Position == Position.BB);
        var sb = round.Players.FirstOrDefault(x => x.Position == Position.SB);
        _builder.AppendLine($"{sb.Name}: posts small blind ${round.SmallBlind.ToString("0.00").Replace(",", ".")}");
        _builder.AppendLine($"{bb.Name}: posts big blind ${round.BigBlind.ToString("0.00").Replace(",", ".")}");
        _builder.AppendLine("*** HOLE CARDS ***");
        foreach (var player in round.Players)
            _builder.AppendLine($"Dealt to {player.Name} [{player.Hand.GetStringFromHand().Insert(2, " ")}]");
    }

    public void SaveHistoryToFile(string path)
    {
        var contents = "";
        ; // path to file
        if (File.Exists(path)) contents = File.ReadAllText(path);

        using (var fs = File.Create(path))
        {
            // writing data in string
            if (contents.Length > 0)
            {
                _builder.AppendLine();
                _builder.AppendLine();
            }

            var info = new UTF8Encoding(true).GetBytes(contents + _builder);
            fs.Write(info, 0, info.Length);

            // writing data in bytes already
            byte[] data = { 0x0 };
            fs.Write(data, 0, data.Length);
        }
    }

    public void PlayerRaises(Player player, decimal difference, decimal overall, Round round)
    {
        var overallString = (overall * round.BigBlind).ToString("0.00").Replace(",", ".");
        var allinString = player.Chips == 0 ? "and is all-in" : "";
        var differenceString = (difference * round.BigBlind).ToString("0.00").Replace(",", ".");
        _builder.AppendLine($"{player.Name}: raises ${differenceString} to ${overallString} {allinString}");
    }

    public void PlayerBets(Player player, decimal size, Round round)
    {
        var overallString = (size * round.BigBlind).ToString("0.00").Replace(",", ".");
        var allinString = player.Chips == 0 ? "and is all-in" : "";
        _builder.AppendLine($"{player.Name}: bets ${overallString} {allinString}");
    }

    public void PlayerCalls(Player player, decimal toCallAmount, Round round)
    {
        var overallString = (toCallAmount * round.BigBlind).ToString("0.00").Replace(",", ".");
        _builder.AppendLine($"{player.Name}: calls ${overallString}");
    }

    public void PlayerCallsAllin(Player player, decimal toCallAmount, decimal chips, Round round)
    {
        var chipsString = (chips * round.BigBlind).ToString("0.00").Replace(",", ".");
        _builder.AppendLine($"{player.Name}: calls ${chipsString} and is all-in");
        if (chips < toCallAmount)
        {
            var returnString = ((toCallAmount - chips) * round.BigBlind).ToString("0.00").Replace(",", ".");
            _builder.AppendLine(
                $"Uncalled bet (${returnString}) returned to {round.PlayersInHand.First(x => x.Name != player.Name).Name}");
        }
    }

    public void PlayerFolds(Player player)
    {
        _builder.AppendLine($"{player.Name}: folds");
    }


    public void PlayerChecks(Player player)
    {
        _builder.AppendLine($"{player.Name}: checks");
    }

    public void HandleWin( decimal returnAmount, Round round, bool withShowDown)
    {
        var winner = round.PlayersInHand;
        if (returnAmount > 0)
        {
            var returnString = (returnAmount * round.BigBlind).ToString("0.00").Replace(",", ".");
            _builder.AppendLine($"Uncalled bet (${returnString}) returned to {winner.First()?.Name}");
        }

        var potString = (round.Pot * round.BigBlind - returnAmount * round.BigBlind).ToString("0.00").Replace(",", ".");

        if (withShowDown)
        {
            winner = HandleWinShowdown(round.PlayersInHand.First(), round.PlayersInHand.Last(), round.Board, round).ToList() ?? winner;
        }
        else
        {
            _builder.AppendLine($"{winner.First()?.Name} collected ${potString} from pot");
            _builder.AppendLine($"{winner.First()?.Name}: doesn´t show hand");
        }

        _builder.AppendLine("*** SUMMARY ***");
        _builder.AppendLine($"Total pot ${potString} | Rake $0 ");
        if (round.Board.Any())
        {
            var boardString = "";
            foreach (var card in round.Board)
            {
                boardString += " ";
                boardString += card.GetStringFromCard();
            }

            _builder.AppendLine($"Board [{boardString.Substring(1)}]");
        }

        var seat = 1;
        foreach (var p in round.Players)
        {
            var isWinner = winner.Contains( p );
            var posString = p.Position == Position.BTN ? "(button) " :
                p.Position == Position.SB ? "(small blind) " :
                p.Position == Position.BB ? "(big blind) " : "";

            var didNotBet = p.MaxStreetReached == Street.PreFlop && p.ChipsInvestedInRound == 0
                ? " (didn´t bet)"
                : "";

            if (!p.PlayerInHand)
            {
                _builder.AppendLine(
                    $"Seat {seat}: {p.Name} {posString}folded {StreetToString(p.MaxStreetReached)}{didNotBet}");
            }
            else if (isWinner)
            {
                if (withShowDown)
                {
                    var playerHand = round.Board.ToList();
                    playerHand.AddRange(new List<Card> { p.Hand.Card1, p.Hand.Card2 });
                    var resultPlayer1 = ResultBusinessHandler.GetResultFromCards(playerHand);
                    _builder.AppendLine(
                        $"Seat {seat}: {p.Name} {posString} showed [{p.Hand.GetStringFromHand()}] and won (${potString}) with {resultPlayer1.Message}");
                }
                else
                {
                    _builder.AppendLine($"Seat {seat}: {p.Name} {posString}collected ${potString}");
                }
            }
            else if (!isWinner)
            {
                var playerHand = round.Board.ToList();
                playerHand.AddRange(new List<Card> { p.Hand.Card1, p.Hand.Card2 });
                var resultPlayer1 = ResultBusinessHandler.GetResultFromCards(playerHand);
                _builder.AppendLine(
                    $"Seat {seat}: {p.Name} {posString} showed [{p.Hand.GetStringFromHand()}] and lost with {resultPlayer1.Message}");
            }

            seat++;
        }

        _builder.AppendLine();
    }

    private IEnumerable<Player> HandleWinShowdown(Player playerOne, Player playerTwo, List<Card> board, Round round)
    {
        var playerOneHand = board.ToList();
        playerOneHand.AddRange(new List<Card> { playerOne.Hand.Card1, playerOne.Hand.Card2 });
        var resultPlayer1 = ResultBusinessHandler.GetResultFromCards(playerOneHand);
        var playerTwoHand = board.ToList();
        playerTwoHand.AddRange(new List<Card> { playerTwo.Hand.Card1, playerTwo.Hand.Card2 });
        var resultPlayer2 = ResultBusinessHandler.GetResultFromCards(playerTwoHand);
        _builder.AppendLine("*** SHOW DOWN ***");
        _builder.AppendLine($"{playerOne.Name}: shows [{playerOne.Hand.GetStringFromHand()}] ({resultPlayer1.Message})");
        _builder.AppendLine($"{playerTwo.Name}: shows [{playerTwo.Hand.GetStringFromHand()}] ({resultPlayer2.Message})");
        var potString = "";
        if (resultPlayer1.Rating > resultPlayer2.Rating)
        {
             potString = (round.Pot * round.BigBlind).ToString("0.00").Replace(",", ".");
            _builder.AppendLine($"{playerOne.Name} collected (${potString}) from the pot");
            return new List<Player> { playerOne };
        }

        if (resultPlayer1.Rating < resultPlayer2.Rating)
        {
            potString = (round.Pot * round.BigBlind).ToString("0.00").Replace(",", ".");
            _builder.AppendLine($"{playerTwo.Name} collected (${potString}) from the pot");
            return new List<Player> { playerTwo };
        }
             potString = (round.Pot / 2 * round.BigBlind).ToString("0.00").Replace(",", ".");
            _builder.AppendLine($"{playerOne.Name} collected (${potString}) from the pot");
            _builder.AppendLine($"{playerTwo.Name} collected (${potString}) from the pot");
            return new List<Player> { playerOne,playerTwo };
        
    }


    public string StreetToString(Street street)
    {
        switch (street)
        {
            case Street.PreFlop:
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

        if (board.Count == 5) board1 += $" {board[board.Count - 2].GetStringFromCard()}";

        var board2 = board.Count == 3 ? "" : $"[{board.Last().GetStringFromCard()}]";

        _builder.AppendLine($"*** {street.ToUpper()} *** [{board1}] {board2}");
    }
}