// See https://aka.ms/new-console-template for more information

using Poker.Infrastructure.HistoryBuilder;
using Poker.Infrastructure.Models;
using Poker.Pio.Connection;

var solver = new SolverConnection(@"C:\PioSolver\PioSOLVER2-pro.exe");
var baseCFRLocation = @"W:";
var handAmount = Console.ReadLine();

for (var i = 0; i < int.Parse(handAmount); i++)
{
    var round = new Round(0.25m, 0.5m, 0.5m);
    var preflopRound = new PreflopRound();
    var historyBuilder = new HistoryBuilder();
    historyBuilder.BuildHeader(round);
    try
    {
        round = preflopRound.PlayPreflop(round, historyBuilder);
    }
    catch (KeyNotFoundException e)
    {
        Console.WriteLine($"Betting Pattern not found: {e.Message}");
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

            var postflopRound = new PostflopRound(round, baseCFRLocation);
            postflopRound.PlayPostflop(historyBuilder, solver);
            break;
        case > 2:
            Console.WriteLine($"More than one Player:  {round.BettingPattern}");
            continue;
    }
    historyBuilder.SaveHistoryToFile("");


}
