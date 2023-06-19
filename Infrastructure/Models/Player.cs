using Poker.Data;
using Poker.Infrastructure.Enums;
using Poker.Pio.Connection;
using Poker.Pio.Util;

namespace Poker.Infrastructure.Models;

public class Player
{
    public Hand Hand { get; set; }
    public string Name { get; set; }
    public Position Position { get; set; }
    public decimal Chips { get; set; }
    public bool PlayerInHand { get; set; } = true;

    public decimal ChipsInvestedInRound { get; set; }

    public Street MaxStreetReached { get; private set; } = Street.Preflop;

    public HandRange ActualRange { get; set; }

    public void NextStreet()
    {
        MaxStreetReached++;
    }

    public Decision MakePreflopPlay(int betRound, List<Player?> playersInGame, string bettingPattern, out string? path)
    {
        var ranges = CarrotsRanges.GetRanges();
        var ownRanges = ranges.Where(x => x.BettingPattern == bettingPattern);
        var betFrequency = 0m;
        var callFrequency = 0m;
        var betSize = 0m;
        path = null;

        if (!ownRanges.Any())
        {
            throw new KeyNotFoundException(bettingPattern);
        }
        var p = bettingPattern;
        var pos = Position;
        var h = Hand.GetSortedStringFromHand();

        foreach (var actionPossibility in ownRanges)
        {
            var handRange = actionPossibility.Frequencies.FirstOrDefault(x => x.Hand.GetSortedStringFromHand() == Hand.GetSortedStringFromHand());

            if (handRange == null) continue;
            if (actionPossibility.BetSize != 0)
            {
                //<0 bedeutet all in
                betFrequency =  handRange.Value;
                betSize = actionPossibility.BetSize < 0 ? Chips+ChipsInvestedInRound : actionPossibility.BetSize;
               
            }
            else
            {
                callFrequency = handRange.Value;
            }

        }

        var rdm = new Random().Next(100);

        if (rdm <= betFrequency * 100 && betFrequency >0)
        {
            ActualRange = ownRanges.FirstOrDefault(x => x.BetSize > 0);
            return new Decision(DecisionKind.Bet, betSize);
        }

        if (rdm <= betFrequency * 100 + callFrequency * 100 &&  callFrequency >0)
        {
            ActualRange = ownRanges.FirstOrDefault(x => x.BetSize == 0);
            path = ActualRange?.Path;
            return new Decision(DecisionKind.Call, 0);
        }


        PlayerInHand = false;
        return new Decision(DecisionKind.Fold, 0);
    }

    public Decision MakePostFlopPlay(SolverConnection solver, ref string nodeString, Dictionary<CardColor, CardColor> solverConversion, bool hasOpenCall)
    {

        var options = StrategyUtil.GetOptions(solver, nodeString);
        var handString = Hand.Convert(solverConversion).GetStringFromHandSorted();
        var strategy = StrategyUtil.GetStrategies(solver, nodeString).FirstOrDefault(x => x.Contains(handString));
        var selectedOption = "";
        if (strategy != null)
        {
            var decisionArray = strategy.Split(":   ")[1].Split("  ");

            var rdm = new Random().Next(100);
            var i = 0;
            while (rdm > Decimal.Parse(decisionArray[i]) * 100)
            {
                rdm -= Convert.ToInt32(Decimal.Parse(decisionArray[i]) * 100);
                i++;
            }
            selectedOption = options[i];
            nodeString += ":" + selectedOption;
        }
        if (strategy != null && selectedOption[0] == 'b')
        {
            var value = decimal.Parse(selectedOption.Substring(1)) / 10;
            value = Math.Min(value, Chips);
            return new Decision(DecisionKind.Bet, value);
        }
        if (strategy != null && selectedOption[0] == 'c')
        {
            return hasOpenCall ? new Decision(DecisionKind.Call, 0) : new Decision(DecisionKind.Check, 0);
        }
        if(strategy == null)
        {
            var i = 1;
        }
        PlayerInHand = false;
        return new Decision(DecisionKind.Fold, 0);
    }

}


