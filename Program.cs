// See https://aka.ms/new-console-template for more information

using Poker.Infrastructure.HistoryBuilder;
using Poker.Infrastructure.Models;
using Poker.Pio.Connection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

SolverConnection solver = new SolverConnection(@"C:\PioSolver\PioSOLVER2-pro.exe");


AppDomain.CurrentDomain.ProcessExit += new EventHandler((i,p)=> CurrentDomainProcessExit(solver));

static void CurrentDomainProcessExit( SolverConnection s)
{
    s.Disconnect();
    Console.WriteLine("Solver stopped.");
}


var baseCFRLocation = @"W:";
var handAmount = Console.ReadLine();
var historyBuilder = new HistoryBuilder();
Stopwatch sw = new Stopwatch();
sw.Start();
for (var i = 0; i < int.Parse(handAmount); i++)
{
   if(i%100 == 0)
    {
        Console.WriteLine($"running time: {sw.Elapsed.ToString()}");
    }
    var round = new Round(0.25m, 0.5m);
    var preFlopRound = new PreflopRound();
    historyBuilder.BuildHeader(round);
    try
    {
        round = preFlopRound.PlayPreflop(round, historyBuilder);
    }
    catch (KeyNotFoundException e)
    {
        Console.WriteLine($"Betting Pattern not found: {e.Message}");
        i--;
        historyBuilder.Reset();
        continue;
    }
    catch (Exception e)
    {
        i--;
        continue;
    }


    try
    {
        switch (round.PlayersInHand.Count)
        {
            case 1:
                Console.WriteLine($"Hand History {i}/{handAmount} created");
                break;
            case 2:

                var postFlopRound = new PostflopRound(round, baseCFRLocation);
                postFlopRound.PlayPostflop(historyBuilder, solver);
                historyBuilder.SaveHistoryToFile("");
                Console.WriteLine($"Hand History {i}/{handAmount} created");
                break;

            case > 2:
                Console.WriteLine($"More than one Player:  {round.BettingPattern}");
                i--;
                break;

        }
    }
    catch (Exception)
    {
        continue;
    }
    finally
    {
        historyBuilder.Reset();
        
    }

}





