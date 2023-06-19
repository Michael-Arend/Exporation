// See https://aka.ms/new-console-template for more information

using Poker.Infrastructure.HistoryBuilder;
using Poker.Infrastructure.Models;
using Poker.Pio.Connection;
using System.Diagnostics;
using System.Timers;

var solver = new SolverConnection(@"C:\PioSolver\PioSOLVER2-pro.exe");
var baseCFRLocation = @"W:";
var handAmount = Console.ReadLine();
var historyBuilder = new HistoryBuilder();
Stopwatch sw = new Stopwatch();
sw.Start();
for (var i = 0; i < int.Parse(handAmount); i++)
{
   if(i%100 == 0)
    {
        Console.WriteLine(sw.Elapsed.ToString());
    }
    var round = new Round(0.25m, 0.5m);
    var preflopRound = new PreflopRound();
    historyBuilder.BuildHeader(round);
    try
    {
        round = preflopRound.PlayPreflop(round, historyBuilder);
    }
    catch (KeyNotFoundException e)
    {
        Console.WriteLine($"Betting Pattern not found: {e.Message}");
        historyBuilder.Reset();
        continue;
    }
    catch (Exception e)
    {
        continue;
    }

    switch (round.PlayersInHand.Count)
    {
        case 1:
            break;
        case 2:
            try
            {
                var postflopRound = new PostflopRound(round, baseCFRLocation);
                postflopRound.PlayPostflop(historyBuilder, solver);
                historyBuilder.SaveHistoryToFile("");
                break;
            }
            catch (Exception)
            {
                continue;
            }
            finally
            {
                historyBuilder.Reset();
            }
        case > 2:
            Console.WriteLine($"More than one Player:  {round.BettingPattern}");
            historyBuilder.Reset();
            continue;
    }


}
