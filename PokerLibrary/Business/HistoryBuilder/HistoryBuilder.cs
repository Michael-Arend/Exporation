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

    public void AppendNewString(string str)
    {
        _builder.Append(str);
    }

    public void BuildHeader(Round round, int currentHand, string baseNumber)
    {
        var rdm = new Random();

        baseNumber += currentHand.ToString("D6");

        var time = DateTime.Now.AddSeconds(currentHand * 20).AddDays(-1);
        _builder.AppendLine(
            $"PokerStars Zoom Hand #{baseNumber}:  Hold'em No Limit (${(round.BigBlind / 2).ToString("0.00").Replace(",", ".")}/${round.BigBlind.ToString("0.00").Replace(",", ".")}) - {time.Year}/{time.Month.ToString("00")}/{time.Day.ToString("00")} {time.Hour}:{time.Minute.ToString("00")}:{time.Second.ToString("00")} CET");
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
        foreach (var player in round.Players.Where(x => x.Name == "HeroGto"))
            _builder.AppendLine($"Dealt to {player.Name} [{player.Hand.GetStringFromHand()}]");
    }

    public int SaveHistoryToFile(string path, bool sendOverallAmount = false)
    {
        var contents = " ";
        ; // path to file
        if (File.Exists(path)) contents = File.ReadAllText(path);
        var amount = sendOverallAmount ? contents.Split("PokerStars").Count() + 1 : 0;

        using (var fs = File.Create(path))
        {
            // writing data in string
            if (contents.Length > 0)
            {
                _builder.AppendLine();
                _builder.AppendLine();
            }

            var info = new UTF8Encoding(true).GetBytes(contents.Substring(0, contents.Length - 1) + _builder);
            fs.Write(info, 0, info.Length);

            // writing data in bytes already
            byte[] data = { 0x0 };
            fs.Write(data, 0, data.Length);
        }
        return amount;

    }

    public void CreateNewHistoryFile(string path)
    {
        var purePath = path.Substring(path.Length - 5);
        var i = 1;

        while (File.Exists(purePath + i + ".txt")){
            i++;
        }
        System.IO.File.Move("path", purePath + i + ".txt");
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
            _builder.AppendLine($"{winner.First()?.Name}: doesn't show hand");
            winner.First().MoneyWon= round.Pot * round.BigBlind - returnAmount * round.BigBlind;
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
                ? " (didn't bet)"
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
                        $"Seat {seat}: {p.Name} {posString}showed [{p.Hand.GetStringFromHand()}] and won (${p.MoneyWon}) with {resultPlayer1.Message}");
                }
                else
                {
                    _builder.AppendLine($"Seat {seat}: {p.Name} {posString}collected (${p.MoneyWon})");
                }
            }
            else if (!isWinner)
            {
                var playerHand = round.Board.ToList();
                playerHand.AddRange(new List<Card> { p.Hand.Card1, p.Hand.Card2 });
                var resultPlayer1 = ResultBusinessHandler.GetResultFromCards(playerHand);
                _builder.AppendLine(
                    $"Seat {seat}: {p.Name} {posString}showed [{p.Hand.GetStringFromHand()}] and lost with {resultPlayer1.Message}");
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
            playerOne.MoneyWon = round.Pot * round.BigBlind;
            _builder.AppendLine($"{playerOne.Name} collected ${potString} from the pot");
            return new List<Player> { playerOne };
        }

        if (resultPlayer1.Rating < resultPlayer2.Rating)
        {
            potString = (round.Pot * round.BigBlind).ToString("0.00").Replace(",", ".");
            _builder.AppendLine($"{playerTwo.Name} collected ${potString} from the pot");
            playerTwo.MoneyWon = round.Pot * round.BigBlind;
            return new List<Player> { playerTwo };
        }
        return HandleSplitPot(playerOne, playerTwo, round);
         

    }

    private IEnumerable<Player> HandleSplitPot(Player playerOne, Player playerTwo, Round round)
    {
        var potCent = round.Pot * round.BigBlind * 100;
        decimal roundHalf = Math.Floor(potCent / 2);
        decimal OOPPlayer = potCent % 2 +roundHalf;
        var IpString = (roundHalf /100).ToString("0.00").Replace(",", ".");
        var OopString = (OOPPlayer / 100).ToString("0.00").Replace(",", ".");
        _builder.AppendLine($"{playerOne.Name} collected (${IpString}) from the pot");
        _builder.AppendLine($"{playerTwo.Name} collected (${OopString}) from the pot");
        playerOne.MoneyWon = roundHalf / 100;
        playerTwo.MoneyWon = OOPPlayer / 100;
        return new List<Player> { playerOne, playerTwo };
    }



    public string StreetToString(Street street)
    {
        switch (street)
        {
            case Street.PreFlop:
                return "before Flop";
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