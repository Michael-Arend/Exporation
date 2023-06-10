using Client.Util;
using Poker.Infrastructure.Helper.Extensions;
using Poker.Pio.Connection;
using System.IO;
using System.Text;

namespace Poker.Infrastructure.Models;

public class PostflopRound
{
    private Round _round;
    private string treeFile;
    private string nodeString;
    Dictionary<CardColor, CardColor> solverConversion;

    public PostflopRound(Round round, string fileLocation)
    {
        _round = round;
        treeFile = fileLocation + round.TreePath;
        nodeString = @"r:0";
        solverConversion = new Dictionary<CardColor, CardColor>();
    }


    public Round PlayPostflop(HistoryBuilder.HistoryBuilder builder, SolverConnection solver)
    {
        var returnAmount = 0m;
        returnAmount = PlayStreetPostFlop(solver, "flop", builder);
        if (_round.PlayersInHand.Count > 1)
        {

           returnAmount =  PlayStreetPostFlop(solver, "turn", builder);
        }
        if (_round.PlayersInHand.Count > 1)
        {
            returnAmount =  PlayStreetPostFlop(solver, "river", builder);
        }
            builder.HandleWin(_round.PlayersInHand.First(), _round.PlayersInHand.Count > 1 ? 0 : returnAmount, _round, _round.PlayersInHand.Count >1);
        return _round;
    }

    public decimal PlayStreetPostFlop(SolverConnection solver, string street, HistoryBuilder.HistoryBuilder builder)
    {
        _round.NextBoardCards();
        builder.HandleNewStreet(street, _round.Board);
        if (_round.PlayersInHand.Any(x => x.Chips == 0))
        {
            return 0;
        }
        var treeString = "";
        if (_round.Board.Count == 3)
        {
            CreateSolverConversion(_round.Board);
             treeString = GetTreeString(_round.Board);
            // update Convert pattern to pio
            try
            {
                TreeUtil.LoadTree(solver, treeFile + treeString + ".cfr");

            }
            catch (Exception)
            {
                Console.WriteLine($"Tree not Found: {treeFile + GetTreeString(_round.Board) + ".cfr"}");
            }
        }
        else
        {
            nodeString += ":";
            nodeString += _round.Board.Last().GetStringFromCard()[0];
            nodeString += solverConversion.TryGetValue(_round.Board.Last().Color, out var newColor) ? newColor.GetCharFromColor() : _round.Board.Last().GetStringFromCard()[1];
        }

        var streetEnded = false;
        var checkedCount = 0;
        var toCallAmount = 0m;
        var latestRaiseAmount = 0.5m;
        while (!streetEnded)
        {
        
            var decision = new Decision(DecisionKind.Fold,0);
            try
            {
                decision = _round.PlayerToAct.MakePostFlopPlay(solver, ref nodeString, solverConversion, toCallAmount > 0);
            }
            catch (Exception)
            {
                Console.WriteLine($"Tree not Found: {treeFile + treeString + ".cfr"}");
                SaveMissingSolves($"{treeFile + treeString + ".cfr"}");
            }
            _round.UpdateBettingPatternPostflop(decision);

            if (decision.Kind == DecisionKind.Bet)
            {
                _round.HandleBettingAndCalling(_round.PlayerToAct, decision.Amount - _round.PlayerToAct.ChipsInvestedInRound);
                if (toCallAmount == 0)
                {
                    builder.PlayerBets(_round.PlayerToAct, decision.Amount, _round);
                }
                else
                {
                    builder.PlayerRaises(_round.PlayerToAct, decision.Amount - toCallAmount, decision.Amount, _round);
                }
                
                latestRaiseAmount = decision.Amount - toCallAmount;
                toCallAmount = decision.Amount;

            }
            if (decision.Kind == DecisionKind.Call)
            {
                
                if(_round.PlayerToAct.Chips <= toCallAmount - _round.PlayerToAct.ChipsInvestedInRound)
                {
                    //todo Call Allin in builder
                    builder.PlayerCallsAllin(_round.PlayerToAct, toCallAmount, _round.PlayerToAct.Chips, _round);
                    _round.HandleBettingAndCalling(_round.PlayerToAct, _round.PlayerToAct.Chips);
                }
                else
                {
                    builder.PlayerCalls(_round.PlayerToAct, toCallAmount - _round.PlayerToAct.ChipsInvestedInRound, _round);
                    _round.HandleBettingAndCalling(_round.PlayerToAct, toCallAmount - _round.PlayerToAct.ChipsInvestedInRound);
                }
            }
            if (decision.Kind == DecisionKind.Check)
            {
                builder.PlayerChecks(_round.PlayerToAct);
            }
            if (decision.Kind == DecisionKind.Fold)
            {
                builder.PlayerFolds(_round.PlayerToAct);
            }

            checkedCount = decision.Kind == DecisionKind.Check ? checkedCount + 1 : 0;
            if (decision.Kind == DecisionKind.Call || decision.Kind == DecisionKind.Fold || checkedCount == 2)
            {
                streetEnded = true;
            
            }
            _round.NextPlayer();
        }
        _round.PlayersInHand.ForEach(x =>
        {
            x.ChipsInvestedInRound = 0;
            x.NextStreet();
        });
        return latestRaiseAmount;
    }

    private string GetTreeString(IEnumerable<Card> board)
    {
        var flop = board.Take(3);

        flop = OrderFlopForSolver(flop.ToList());

        var output = "";
        foreach (var card in flop)
        {
            output += card.GetStringFromCard()[0];
            output += solverConversion.TryGetValue(card.Color, out var newColor) ? newColor.GetCharFromColor() : card.GetStringFromCard()[1];
        }
        return output;
    }

    private IEnumerable<Card> OrderFlopForSolver(List<Card> flop)
    {
       flop = flop.OrderByDescending(x=> x.Value).ToList();
        if(flop[1].Value == flop[2].Value && flop.First().Color == flop.Last().Color)
        {
            var mid = flop[1];
            flop.Remove(flop[1]);
            flop.Add(mid);
        }

        if (flop[0].Value == flop[1].Value && flop[1].Color == flop[2].Color)
        {
           flop = new List<Card> { flop[1], flop[0], flop[2] };
        }

        return flop;
    }

    private void CreateSolverConversion(IEnumerable<Card> Board)
    {
        var flop = Board.Take(3);
        flop = OrderFlopForSolver(flop.ToList());
        var highColor = flop.FirstOrDefault()?.Color ?? CardColor.C;
        var secondColor = flop.ToList()[1].Color;
        var thirdcolor = flop.ToList()[2].Color;
        var openForChange = new List<CardColor> { CardColor.H, CardColor.D, CardColor.C };
        var toChange = new List<CardColor>();
        var hasChanged = new List<CardColor>();

        if (highColor != CardColor.S)
        {
            solverConversion.Add(key: highColor, CardColor.S);
            toChange.Add(CardColor.S);
            hasChanged.Add(highColor);
        }


        if (secondColor != highColor)
        {
            openForChange.Remove(CardColor.H);
            if (secondColor != CardColor.H)
            {
                solverConversion.Add(key: secondColor, CardColor.H);
                toChange.Add(CardColor.H);
                hasChanged.Add(secondColor);
            }

        }

        if (thirdcolor != highColor && thirdcolor != secondColor)
        {
            var cardColor = highColor == secondColor ? CardColor.H : CardColor.D;
            openForChange.Remove(cardColor);
            if (thirdcolor != cardColor)
            {
                solverConversion.Add(key: thirdcolor, cardColor);
                toChange.Add(cardColor);
                hasChanged.Add(thirdcolor);
            }

        }

        foreach (var ch in hasChanged)
        {
            toChange.Remove(ch);
        }


        while (toChange.Count > 0)
        {
            var changeTo = openForChange.FirstOrDefault(x => x != toChange[0]);
            solverConversion.Add(toChange[0], changeTo);
            openForChange.Remove(changeTo);
            toChange.Remove(toChange[0]);
            if (!hasChanged.Any(x => x == changeTo))
            {
                toChange.Add(changeTo);
            }
        }
    }



    private static void SaveMissingSolves(string tree)
    {
        var builder = new StringBuilder();
        var contents = "";
        string path = @"C:\PioSolver\missing.txt"; // path to file
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
                builder.AppendLine(tree);
            }
            byte[] info = new UTF8Encoding(true).GetBytes(contents + builder.ToString());
            fs.Write(info, 0, info.Length);

            // writing data in bytes already
            byte[] data = new byte[] { 0x0 };
            fs.Write(data, 0, data.Length);
        }
    }


}