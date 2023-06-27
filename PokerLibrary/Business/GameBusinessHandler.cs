using System.Diagnostics;
using Poker.Pio.Connection;
using PokerLibrary.Infrastructure.Models;

namespace PokerLibrary.Business;

public class GameBusinessHandler : GameBusinessHandler.IGameBusinessHandler
{
    private bool abort;
    private SolverConnection _solver;

    public GameBusinessHandler(string solverPath)
    {
        this.abort = false;
        this._solver = new SolverConnection(solverPath);
    }

    public void Stop()
    {
        abort = true;
        CurrentDomainProcessExit();
        SendMessage("Process stopped.");
      
    }
    void CurrentDomainProcessExit()
    {
        _solver.Disconnect();
        SendMessage("Solver stopped.");
    }



    public async Task Play(IEnumerable<HandRange> ranges, int handAmount, string baseCFRLocation, string handHistoryPath)
    {
        abort = false;
        AppDomain.CurrentDomain.ProcessExit += (i, p) => CurrentDomainProcessExit();

    

        var historyBuilder = new HistoryBuilder.HistoryBuilder();
        var sw = new Stopwatch();
        sw.Start();
        var currentHand = 1;
        while (currentHand < handAmount && !abort)
        {
            if (currentHand % 100 == 0) SendMessage($"running time: {sw.Elapsed.ToString()}");
            var round = new Round(0.25m, 0.5m);
            historyBuilder.BuildHeader(round);
            try
            {
                round = PreFlopBusinessHandler.PlayPreFlop(round, historyBuilder, ranges);
            }
            catch (KeyNotFoundException e)
            {
                SendMessage($"Betting Pattern not found: {e.Message}");
                currentHand--;
                historyBuilder.Reset();
                continue;
            }
            catch (Exception)
            {
                currentHand--;
                continue;
            }


            try
            {
                switch (round.PlayersInHand.Count)
                {
                    case 1:
                        SendMessage($"Hand History {currentHand}/{handAmount} created");
                        currentHand++;
                        break;
                    case 2:


                        PostFlopBusinessHandler.PlayPostFlop(round, historyBuilder, _solver, baseCFRLocation);
                        historyBuilder.SaveHistoryToFile(handHistoryPath);
                        SendMessage($"Hand History {currentHand}/{handAmount} created");
                        currentHand++;
                        break;

                    case > 2:
                        SendMessage($"More than one Player:  {round.BettingPattern}");
                        currentHand--;
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
        Task Play(IEnumerable<HandRange> ranges, int handAmount, string baseCFRLocation,  string handHistoryPath);
    }
}