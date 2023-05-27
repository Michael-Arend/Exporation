using System.Text;

namespace Poker.Infrastructure.Models;

public class PreflopRound
{
    private readonly int _bettingRound = 0;
    private  Position _endActionPlayer = Position.BB;
    private Round _round;
    private decimal toCallAmount =0;
    private decimal latestRaiseAmount = 0;

    public Round PlayPreflop(Round round, HistoryBuilder.HistoryBuilder builder)
    {
        _round = round;
        StartPreflopRound();
        var roundEnded = false;
        while (!roundEnded && _round.PlayersInHand.Count >1)
        {
            _round.NextPlayer();
            var decision =
                _round.PlayerToAct.MakePreflopPlay(_bettingRound, _round.PlayersInHand, _round.BettingPattern, out var path);
            _round.TreePath = path ?? _round.TreePath;

            _round.UpdateBettingPattern(decision);
            if (decision.Kind == DecisionKind.Bet)
            {
                builder.PlayerRaises(round.PlayerToAct, decision.Amount - toCallAmount, decision.Amount, round);
                _round.HandleBettingAndCalling(_round.PlayerToAct, decision.Amount);
                _endActionPlayer = _round.FindLastToAct();
                latestRaiseAmount = decision.Amount - toCallAmount;
                toCallAmount = decision.Amount;
                
            }
            else if (decision.Kind == DecisionKind.Fold)
            {
                builder.PlayerFolds(round.PlayerToAct);
            }
            else if (decision.Kind == DecisionKind.Call)
            {
                builder.PlayerCalls(round.PlayerToAct, toCallAmount - _round.PlayerToAct.ChipsInvestedInRound, _round);
                _round.HandleBettingAndCalling(_round.PlayerToAct, toCallAmount- _round.PlayerToAct.ChipsInvestedInRound);
            }

            roundEnded = _round.PlayerToAct.Position == _endActionPlayer;
        }

        if (_round.PlayersInHand.Count == 1)
        {
            builder.HandleWinNoShowdown(_round.PlayersInHand.First(),
                latestRaiseAmount, _round);
        }

        _round.PlayersInHand.ForEach(x=>
        {
            x.ChipsInvestedInRound = 0;
            x.NextStreet();
        });
        return _round;

    }


    public void StartPreflopRound()
    {
        PostSmallBlind();
        PostBigBlind();
    }

    public void PostSmallBlind()
    {
        _round.PlayerToAct.Chips -= _round.SmallBlind;
        _round.Pot += _round.SmallBlind;
        _round.PlayerToAct.ChipsInvestedInRound = _round.SmallBlind;
        _round.NextPlayer();
    }

    public void PostBigBlind()
    {
        _round.PlayerToAct.Chips -= _round.BigBlind;
        _round.Pot += _round.BigBlind;
        toCallAmount = _round.BigBlind;
        _round.PlayerToAct.ChipsInvestedInRound = _round.BigBlind;
    }
}