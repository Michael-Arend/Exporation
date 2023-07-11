using PokerLibrary.Infrastructure.Enums;
using PokerLibrary.Infrastructure.Models;

namespace PokerLibrary.Business;

internal class PreFlopBusinessHandler
{
    public static Round PlayPreFlop(Round round, HistoryBuilder.HistoryBuilder builder, IEnumerable<HandRange> ranges)
    {
        var endActionPlayer = Position.BB;
        var (latestRaiseAmount, toCallAmountInBb) = StartPreFlopRound(round);

        var roundEnded = false;
        while (!roundEnded && round.PlayersInHand.Count > 1)
        {
            if (round.PlayerToAct == null) continue;

            var decision =
                GetDecision(round.PlayerToAct, round.BettingPattern,
                    out var path, ranges);
            round.TreePath = path ?? round.TreePath;

            round.UpdateBettingPatternPreflop(decision);

            switch (decision.Kind)
            {
                case DecisionKind.Bet:
                    round.HandleBettingAndCalling(round.PlayerToAct,
                        decision.Amount - round.PlayerToAct.ChipsInvestedInRound);
                    builder.PlayerRaises(round.PlayerToAct, decision.Amount - toCallAmountInBb, decision.Amount, round);
                    endActionPlayer = round.FindLastToAct();
                    latestRaiseAmount = decision.Amount - toCallAmountInBb;
                    toCallAmountInBb = decision.Amount;
                    break;
                case DecisionKind.Fold:
                    builder.PlayerFolds(round.PlayerToAct);
                    break;
                case DecisionKind.Call:
                    builder.PlayerCalls(round.PlayerToAct, toCallAmountInBb - round.PlayerToAct.ChipsInvestedInRound,
                        round);
                    round.HandleBettingAndCalling(round.PlayerToAct,
                        toCallAmountInBb - round.PlayerToAct.ChipsInvestedInRound);
                    break;
                case DecisionKind.Check:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            roundEnded = round.PlayerToAct.Position == endActionPlayer;
            round.NextPlayer();
        }

        if (round.PlayersInHand.Count == 1)
            builder.HandleWin(
                latestRaiseAmount, round, false);

        round.PlayersInHand.ForEach(x =>
        {
            x.ChipsInvestedInRound = 0;
            x.NextStreet();
        });
        return round;
    }

    private static (decimal toCallAmount, decimal latestRaiseAmount) StartPreFlopRound(Round round)
    {
        PostSmallBlind(round);
        return PostBigBlind(round);
    }

    private static void PostSmallBlind(Round round)
    {
        if (round.PlayerToAct == null) return;
        round.PlayerToAct.Chips -= 0.5m;
        round.Pot += 0.5m;
        round.PlayerToAct.ChipsInvestedInRound = 0.5m;
        round.NextPlayer();
    }


    private static  (decimal toCallAmount, decimal latestRaiseAmount) PostBigBlind(Round round)
    {
        if (round.PlayerToAct == null) return (0, 0);
        round.PlayerToAct.Chips -= 1;
        round.Pot += 1;
        round.PlayerToAct.ChipsInvestedInRound = 1;
        round.NextPlayer();
        return (0.5m, 1);
    }

    private static Decision GetDecision(Player player, string bettingPattern, out string? path, IEnumerable<HandRange> ranges)
    {
        var ownRanges = ranges.Where(x => x.BettingPattern == bettingPattern);
        var betFrequency = 0m;
        var callFrequency = 0m;
        var betSize = 0m;
        path = null;

        if (ownRanges == null) throw new KeyNotFoundException(bettingPattern);

        foreach (var actionPossibility in ownRanges)
        {
            var handRange = actionPossibility.Frequencies.FirstOrDefault(x =>
                x.Hand.GetSortedStringFromHand() == player.Hand.GetSortedStringFromHand());

            if (handRange == null) continue;
            if (actionPossibility.BetSize != 0)
            {
                //<0 means all in
                betFrequency = handRange.Value;
                betSize = actionPossibility.BetSize < 0
                    ? player.Chips + player.ChipsInvestedInRound
                    : actionPossibility.BetSize;
            }
            else
            {
                callFrequency = handRange.Value;
            }
        }

        var rdm = new Random().Next(100);

        if (rdm <= betFrequency * 100 && betFrequency > 0)
        {
            player.ActualRange = ownRanges.FirstOrDefault(x => x.BetSize > 0);
            return new Decision(DecisionKind.Bet, betSize);
        }

        if (rdm <= betFrequency * 100 + callFrequency * 100 && callFrequency > 0)
        {
            player.ActualRange = ownRanges.FirstOrDefault(x => x.BetSize == 0);
            path = player.ActualRange?.Path;
            return new Decision(DecisionKind.Call, 0);
        }


        player.PlayerInHand = false;
        return new Decision(DecisionKind.Fold, 0);
    }
}