using System.Text;
using Client.Util;
using Poker.Infrastructure.Helper.Extensions;
using Poker.Pio.Connection;
using Poker.Pio.Util;
using PokerLibrary.Infrastructure.Enums;
using PokerLibrary.Infrastructure.Models;

namespace PokerLibrary.Business;

internal  class PostFlopBusinessHandler
{
    public static Round PlayPostFlop(Round round, HistoryBuilder.HistoryBuilder builder, SolverConnection solver,
        string fileLocation)
    {
        round.NextStreet();
        var returnAmount = 0m;
        var treeFile = fileLocation + round.TreePath;
        var nodeString = @"r:0";
        var solverConversion = new Dictionary<CardColor, CardColor>();

        returnAmount =
            PlayStreetPostFlop(round, solver, "flop", builder, treeFile, ref nodeString, ref solverConversion);
        if (round.PlayersInHand.Count > 1)
        {
            round.NextStreet();
            returnAmount = PlayStreetPostFlop(round, solver, "turn", builder, treeFile, ref nodeString,
                ref solverConversion);
        }

        if (round.PlayersInHand.Count > 1)
        {
            round.NextStreet();
            returnAmount = PlayStreetPostFlop(round, solver, "river", builder, treeFile, ref nodeString,
                ref solverConversion);
        }

        builder.HandleWin( round.PlayersInHand.Count > 1 ? 0 : returnAmount, round,
            round.PlayersInHand.Count > 1);
        return round;
    }

    private static decimal PlayStreetPostFlop(Round round, SolverConnection solver, string street,
        HistoryBuilder.HistoryBuilder builder, string treeFile, ref string nodeString,
        ref Dictionary<CardColor, CardColor> solverConversion)
    {
        round.NextBoardCards();
        builder.HandleNewStreet(street, round.Board);
        if (round.PlayersInHand.Any(x => x.Chips == 0)) return 0;
        var treeString = "";
        if (round.Board.Count == 3)
        {
            treeString = LoadTree(round, solver, treeFile, out solverConversion);
        }
        else
        {
            nodeString += ":";
            nodeString += round.Board.Last().GetStringFromCard()[0];
            nodeString += solverConversion.TryGetValue(round.Board.Last().Color, out var newColor)
                ? newColor.GetCharFromColor()
                : round.Board.Last().GetStringFromCard()[1];
        }

        var latestRaiseAmount =
            PlayStreet(round, solver, builder, treeFile, ref nodeString, solverConversion, treeString);

        round.PlayersInHand.ForEach(player =>
        {
            player.ChipsInvestedInRound = 0;
            player.NextStreet();
        });
        return latestRaiseAmount;
    }

    private static decimal PlayStreet(Round round, SolverConnection solver, HistoryBuilder.HistoryBuilder builder,
        string treeFile,
        ref string nodeString, Dictionary<CardColor, CardColor> solverConversion, string treeString)
    {
        var streetEnded = false;
        var checkedCount = 0;
        var toCallAmount = 0m;
        var latestRaiseAmount = 0m;
        while (!streetEnded)
        {
            Decision decision;
            try
            {
                decision = GetDecision(round.PlayerToAct, solver, ref nodeString, solverConversion, toCallAmount > 0);
            }
            catch (Exception)
            {
                GameBusinessHandler.SendMessage($"Tree not Found: {treeFile + treeString + ".cfr"}");
                SaveMissingSolves($"{treeFile + treeString + ".cfr"}");
                throw new Exception();
            }

            round.UpdateBettingPatternPostflop(decision);

            switch (decision.Kind)
            {
                case DecisionKind.Bet:
                {
                    round.HandleBettingAndCalling(round.PlayerToAct,
                        decision.Amount - round.PlayerToAct.ChipsInvestedInRound);
                    if (toCallAmount == 0)
                        builder.PlayerBets(round.PlayerToAct, decision.Amount, round);
                    else
                        builder.PlayerRaises(round.PlayerToAct, decision.Amount - toCallAmount, decision.Amount, round);

                    latestRaiseAmount = decision.Amount - toCallAmount;
                    toCallAmount = decision.Amount;
                    break;
                }
                case DecisionKind.Call
                    when round.PlayerToAct.Chips <= toCallAmount - round.PlayerToAct.ChipsInvestedInRound:
                    builder.PlayerCallsAllin(round.PlayerToAct, toCallAmount - round.PlayerToAct.ChipsInvestedInRound,
                        round.PlayerToAct.Chips, round);
                    round.HandleBettingAndCalling(round.PlayerToAct, round.PlayerToAct.Chips);
                    break;
                case DecisionKind.Call:
                    builder.PlayerCalls(round.PlayerToAct, toCallAmount - round.PlayerToAct.ChipsInvestedInRound,
                        round);
                    round.HandleBettingAndCalling(round.PlayerToAct,
                        toCallAmount - round.PlayerToAct.ChipsInvestedInRound);
                    break;
                case DecisionKind.Check:
                    builder.PlayerChecks(round.PlayerToAct);
                    break;
                case DecisionKind.Fold:
                    builder.PlayerFolds(round.PlayerToAct);
                    break;
            }

            checkedCount = decision.Kind == DecisionKind.Check ? checkedCount + 1 : 0;
            streetEnded = decision.Kind is DecisionKind.Call or DecisionKind.Fold || checkedCount == 2;
            round.NextPlayer();
        }

        return latestRaiseAmount;
    }

    private static string LoadTree(Round round, SolverConnection solver, string treeFile,
        out Dictionary<CardColor, CardColor> solverConversion)
    {
        solverConversion = CreateSolverConversion(round.Board);
        var treeString = GetTreeString(round.Board, solverConversion);
        // update Convert pattern to pio
        try
        {
            var successful = TreeUtil.LoadTree(solver, treeFile + treeString + ".cfr");
            if (!successful)
            {
                GameBusinessHandler.SendMessage(
                    $"Tree not Found: {treeFile + GetTreeString(round.Board, solverConversion) + ".cfr"}");

                SolveTree(round, solver, treeFile + treeString + ".cfr");
            }
        }
        catch (Exception e)
        {
            throw e;
        }

        return treeString;
    }

    private static string GetTreeString(IEnumerable<Card> board, Dictionary<CardColor, CardColor> solverConversion)
    {
        var flop = board.Take(3);

        flop = OrderFlopForSolver(flop.ToList());

        var output = "";
        foreach (var card in flop)
        {
            output += card.GetStringFromCard()[0];
            output += solverConversion.TryGetValue(card.Color, out var newColor)
                ? newColor.GetCharFromColor()
                : card.GetStringFromCard()[1];
        }

        return output;
    }

    private static Decision GetDecision(Player player, SolverConnection solver, ref string nodeString,
        Dictionary<CardColor, CardColor> solverConversion, bool hasOpenCall)
    {
        var options = StrategyUtil.GetOptions(solver, nodeString);
        var handString = player.Hand.Convert(solverConversion).GetStringFromHandSorted();
        var strategy = StrategyUtil.GetStrategies(solver, nodeString).FirstOrDefault(x => x.Contains(handString));
        var selectedOption = "";
        if (strategy != null)
        {
            var decisionArray = strategy.Split(":   ")[1].Split("  ");

            var rdm = new Random().Next(100);
            var i = 0;
            while (rdm > decimal.Parse(decisionArray[i]) * 100)
            {
                rdm -= Convert.ToInt32(decimal.Parse(decisionArray[i]) * 100);
                i++;
            }

            selectedOption = options[i];
            nodeString += ":" + selectedOption;
        }

        if (strategy != null && selectedOption[0] == 'b')
        {
            var value = decimal.Parse(selectedOption.Substring(1)) / 10;
            value = Math.Min(value, player.Chips);
            return new Decision(DecisionKind.Bet, value);
        }

        if (strategy != null && selectedOption[0] == 'c')
            return hasOpenCall ? new Decision(DecisionKind.Call, 0) : new Decision(DecisionKind.Check, 0);
        if (strategy == null)
        {
            var i = 1;
        }

        player.PlayerInHand = false;
        return new Decision(DecisionKind.Fold, 0);
    }

    private static IEnumerable<Card> OrderFlopForSolver(List<Card> flop)
    {
        flop = flop.OrderByDescending(x => x.Value).ToList();
        if (flop[1].Value == flop[2].Value && flop.First().Color == flop.Last().Color)
        {
            var mid = flop[1];
            flop.Remove(flop[1]);
            flop.Add(mid);
        }

        if (flop[0].Value == flop[1].Value && flop[1].Color == flop[2].Color)
            flop = new List<Card> { flop[1], flop[0], flop[2] };

        return flop;
    }

    private static Dictionary<CardColor, CardColor> CreateSolverConversion(IEnumerable<Card> board)
    {
        var solverConversion = new Dictionary<CardColor, CardColor>();
        var flop = board.Take(3);
        flop = OrderFlopForSolver(flop.ToList());
        var highColor = flop.FirstOrDefault()?.Color ?? CardColor.C;
        var secondColor = flop.ToList()[1].Color;
        var thirdColor = flop.ToList()[2].Color;
        var openForChange = new List<CardColor> { CardColor.H, CardColor.D, CardColor.C };
        var toChange = new List<CardColor>();
        var hasChanged = new List<CardColor>();

        if (highColor != CardColor.S)
        {
            solverConversion.Add(highColor, CardColor.S);
            toChange.Add(CardColor.S);
            hasChanged.Add(highColor);
        }


        if (secondColor != highColor)
        {
            openForChange.Remove(CardColor.H);
            if (secondColor != CardColor.H)
            {
                solverConversion.Add(secondColor, CardColor.H);
                toChange.Add(CardColor.H);
                hasChanged.Add(secondColor);
            }
        }

        if (thirdColor != highColor && thirdColor != secondColor)
        {
            var cardColor = highColor == secondColor ? CardColor.H : CardColor.D;
            openForChange.Remove(cardColor);
            if (thirdColor != cardColor)
            {
                solverConversion.Add(thirdColor, cardColor);
                toChange.Add(cardColor);
                hasChanged.Add(thirdColor);
            }
        }

        foreach (var ch in hasChanged) toChange.Remove(ch);


        while (toChange.Count > 0)
        {
            var changeTo = openForChange.FirstOrDefault(x => x != toChange[0]);
            solverConversion.Add(toChange[0], changeTo);
            openForChange.Remove(changeTo);
            toChange.Remove(toChange[0]);
            if (!hasChanged.Any(x => x == changeTo)) toChange.Add(changeTo);
        }

        return solverConversion;
    }


    private static void SaveMissingSolves(string tree)
    {
        var builder = new StringBuilder();
        var contents = "";
        var path = @"C:\PioSolver\missing.txt"; // path to file
        if (File.Exists(path)) contents = File.ReadAllText(path);

        using (var fs = File.Create(path))
        {
            // writing data in string
            if (contents.Length > 0)
            {
                builder.AppendLine();
                builder.AppendLine(tree);
            }

            var info = new UTF8Encoding(true).GetBytes(contents + builder);
            fs.Write(info, 0, info.Length);

            // writing data in bytes already
            byte[] data = { 0x0 };
            fs.Write(data, 0, data.Length);
        }
    }

    private static void SolveTree(Round _round, SolverConnection solver, string savePath)
    {
        var board =
            $"{_round.Board[0].GetStringFromCard()}{_round.Board[1].GetStringFromCard()}{_round.Board[2].GetStringFromCard()}";
        PostFlopSolves.BuildTree(solver, _round.PlayersInHand, board, _round.Pot);
        PostFlopSolves.Solve(solver, savePath);
    }
}