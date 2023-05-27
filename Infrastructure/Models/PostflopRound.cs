using System.Text;
using Client.Util;
using Poker.Pio.Connection;

namespace Poker.Infrastructure.Models;

public class PostflopRound
{
    private Round _round;
    private string treeFile;
    private string nodeString;
    private List<Card> communityCards;
    public PostflopRound(Round round, string fileLocation)
    {
        _round = round;
        treeFile = fileLocation + round.TreePath;
        nodeString = @"r:0";
    }
  

    public Round PlayPostflop( HistoryBuilder.HistoryBuilder builder,SolverConnection solver)
    {
     //TreeUtil.LoadTree(solver, treeFile);
     PlayFlop(solver);

     return _round;
    }

    public void PlayFlop(SolverConnection solver)
    {
        //todo: deal cards, update treepath
        //todo: update Convert pattern to pio
        Console.WriteLine("PostFlop");
       // _round.PlayerToAct.MakePostFlopPlay(solver, nodeString);

    }


}