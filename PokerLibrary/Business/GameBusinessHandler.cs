using System.Diagnostics;
using Poker.Pio.Connection;
using PokerLibrary.Infrastructure.Models;

namespace PokerLibrary.Business;

public class GameBusinessHandler : GameBusinessHandler.IGameBusinessHandler
{
    public void Play(IEnumerable<HandRange> ranges, int handAmount, string baseCFRLocation, string solverPath, string handHistoryPath)
    {
        var solver = new SolverConnection(solverPath);


        AppDomain.CurrentDomain.ProcessExit += (i, p) => CurrentDomainProcessExit(solver);

        static void CurrentDomainProcessExit(ISolverConnection s)
        {
            s.Disconnect();
            SendMessage("Solver stopped.");
        }


        var historyBuilder = new HistoryBuilder.HistoryBuilder();
        var sw = new Stopwatch();
        sw.Start();
        for (var i = 0; i < handAmount; i++)
        {
            if (i % 100 == 0) SendMessage($"running time: {sw.Elapsed.ToString()}");
            var round = new Round(0.25m, 0.5m);
            historyBuilder.BuildHeader(round);
            try
            {
                round = PreFlopBusinessHandler.PlayPreFlop(round, historyBuilder, ranges);
            }
            catch (KeyNotFoundException e)
            {
                SendMessage($"Betting Pattern not found: {e.Message}");
                i--;
                historyBuilder.Reset();
                continue;
            }
            catch (Exception)
            {
                i--;
                continue;
            }


            try
            {
                switch (round.PlayersInHand.Count)
                {
                    case 1:
                        SendMessage($"Hand History {i}/{handAmount} created");
                        break;
                    case 2:


                        PostFlopBusinessHandler.PlayPostFlop(round, historyBuilder, solver, baseCFRLocation);
                        historyBuilder.SaveHistoryToFile(handHistoryPath);
                        SendMessage($"Hand History {i}/{handAmount} created");
                        break;

                    case > 2:
                        SendMessage($"More than one Player:  {round.BettingPattern}");
                        i--;
                        break;
                }
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                historyBuilder.Reset();
            }
        }
    }

    public static event EventHandler<string> NewMessage;

    internal static void SendMessage(string message)
    {
        NewMessage?.Invoke(null, message);
    }

    public interface IGameBusinessHandler
    {
        void Play(IEnumerable<HandRange> ranges, int handAmount, string baseCFRLocation, string solverPath, string handHistoryPath);
    }
}