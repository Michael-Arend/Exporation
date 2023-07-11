using System.Globalization;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Poker.Pio.Connection;
using PokerLibrary.Business;
using PokerLibrary.Infrastructure.Models;
using PokerLibrary.Pio.Util;

namespace Poker.Pio.Util
{
    public static class PostFlopSolves
    {
        private static List<decimal> bettingSizes = new List<decimal>{0,0.33m,0.75m,1.5m};
        private static decimal accuracy;
        private static StringBuilder script;
        private static bool running;


        public static async Task Solve(SolverConnection solver, string savePath, int seconds = 600)
        {
            try
            {

                GameBusinessHandler.SendMessage($"Manually solving tree...");
                running = true;
                var result = solver.GetResponseFromSolver($"go {seconds} seconds");
                solver.LogEvent += Solver_LogEvent;
                var aTimer = new System.Timers.Timer();
                aTimer.Interval = seconds*1000 + 5000;
                aTimer.Elapsed += (i, o) =>
                {
                    running = false;
                    GameBusinessHandler.SendMessage("Error in Solver.");
                };
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
                while (running)
                {
                }
                aTimer.Enabled = false;
                GameBusinessHandler.SendMessage("Manually solving tree completed");
                solver.LogEvent -= Solver_LogEvent;
                if (!string.IsNullOrEmpty(savePath))
                {
             
                    var saveResult = solver.GetResponseFromSolver($"dump_tree {savePath} no_rivers");
                    GameBusinessHandler.SendMessage(saveResult[0]);

                }
            }
            catch (Exception e)
            {
                GameBusinessHandler.SendMessage(e.Message);
                throw;
            }
        
        }

        private static void Solver_LogEvent(SolverMessageType messageType, string message)
        {
            if (message.Contains("SOLVER"))
            {
                GameBusinessHandler.SendMessage(message);
            }

            if (message.Contains("stopped"))
            {
                running = false;
            }
            
        }

        public static string PioStringFromRange(List<Frequency> frequencies)
        {
            var array = HandOrder.GetHandOrderArray();
            var output = "";

            foreach (var hand in array)
            {
                var fr = frequencies.FirstOrDefault(x => x.Hand.GetSortedStringFromHand() == hand);
                var text = fr != null ? fr.Value.ToString(CultureInfo.InvariantCulture) : "0";
                output += " " + text;
            }

            return output;

        }

        public static void BuildTree(SolverConnection solver,  List<Player> player, string board, decimal pot)
        {
            try
            {
                GameBusinessHandler.SendMessage($"Manually building tree...");
                var oop = player.First();
                var ip = player.Last();
                var effStacks = Math.Round(Math.Min(ip.Chips, oop.Chips) * 10m);
                script = new StringBuilder();
                SetRange(solver, "OOP", PioStringFromRange(oop.ActualRange.Frequencies.ToList()));
                SetRange(solver, "IP", PioStringFromRange(ip.ActualRange.Frequencies.ToList()));
                SetStack(solver, effStacks);
                SetBoard(solver, board);
                SetPot(solver, Math.Round(pot) * 10m);
                SetAccuracy(solver, Math.Round(pot) * 10m);
                SetLines(solver, Math.Round(pot) * 10m, effStacks);
                var i = script.ToString();
                SaveScriptToFile(script);
                var loadScript = solver.GetResponseFromSolver(@"load_script_silent C:\PioSolver\script.txt");
                var loadTree = solver.GetResponseFromSolver("build_tree");
                GameBusinessHandler.SendMessage($"Manually building tree completed");
            }
            catch (Exception e)
            {
                GameBusinessHandler.SendMessage(e.Message);
                throw;
            }
       

        }


        public static void SetRange(SolverConnection solver, string position, string range)
        {
            // var loadTree = solver.GetResponseFromSolver("set_range " + $"{position}" + range);
             script.AppendLine("set_range " + $"{position}" + range);

        }

        public static void SetStack(SolverConnection solver, decimal stack)
        {
            //var loadTree = solver.GetResponseFromSolver("set_eff_stack " + stack);
            script.AppendLine("set_eff_stack " + stack);
        }

        public static void SetBoard(SolverConnection solver, string board)
        {
            // var loadTree = solver.GetResponseFromSolver("set_board " + board);
            script.AppendLine("set_board " + board);
        }

        public static void SetPot(SolverConnection solver,decimal pot)
        {
            //var loadTree = solver.GetResponseFromSolver("    set_pot 0 0" + pot);
            script.AppendLine("set_pot 0 0 " + pot);

        }

        public static void SetAccuracy(SolverConnection solver, decimal pot)
        {
            accuracy = pot * 0.0075m;
            //var loadTree = solver.GetResponseFromSolver("set_accuracy " + accuracy);
            script.AppendLine("set_accuracy " + accuracy);
        }

        public static void SetLines(SolverConnection solver,decimal pot, decimal effStacks)
        {
        
          
                var branches = new List<TreeBranch>();

                var flopBranches = HandleCheck(new TreeBranch { ActualBetSize = 0, ActualCommulated = 0, Pot = pot, Pattern = "", EffStack = effStacks }, false);
                branches.AddRange(flopBranches.Where(x => x.ActualCommulated == effStacks));
                foreach (var tree in flopBranches.Where(x => x.ActualCommulated < effStacks))
                {
                    tree.NodesThisRound = 0;
                    var turnRanges = HandleCheck(tree, false);
                    branches.AddRange(turnRanges.Where(x => x.ActualCommulated == effStacks));
                    foreach (var turnTree in turnRanges.Where(x => x.ActualCommulated < effStacks))
                    {
                        turnTree.NodesThisRound = 0;
                        var riverRanges = HandleCheck(turnTree, false);
                        branches.AddRange(riverRanges.Where(x => x.ActualCommulated == effStacks));

                    }
                }

                var set = branches.Select(x => x.Pattern.Substring(1)).OrderBy(x=> x).ToHashSet();
                solver.GetResponseFromSolver("clear_lines");
                foreach (var line in set)
                {
                    var x = $@"add_line {line}";
                    try
                    {
                        //var loadTree = solver.GetResponseFromSolver(x);
                        script.AppendLine(x);
                }
                    catch (Exception e)
                    {
                        GameBusinessHandler.SendMessage(e.Message);
                    }
                   
            }
        }



     

        private static List<TreeBranch> HandleCheck(TreeBranch tree, bool alreadyChecked)
        {
            var output = new List<TreeBranch>();
         
            foreach (var betting in bettingSizes)
            {
                if (betting == 0)
                {
                    if (alreadyChecked)
                    {
                            output.Add(new TreeBranch { Pattern = tree.Pattern + " " +  tree.ActualCommulated, Pot = tree.Pot, ActualBetSize = tree.ActualBetSize, EffStack = tree.EffStack, ActualCommulated = tree.ActualCommulated});
                    }
                    else
                    {
                        output.AddRange(HandleCheck(
                            new TreeBranch
                            {
                                Pattern = tree.Pattern + " " + tree.ActualCommulated, Pot = tree.Pot,
                                ActualBetSize = tree.ActualBetSize,
                                EffStack = tree.EffStack,
                                ActualCommulated = tree.ActualCommulated,
                                NodesThisRound = tree.NodesThisRound+1,
                            }, true));
                    }
                    
                }
                else
                {
                    var betSize = Math.Round(betting * tree.Pot) > tree.EffStack * 0.67m ? tree.EffStack - tree.ActualCommulated  : Math.Round(betting * tree.Pot);
                    var accumulated = Math.Min(betSize + tree.ActualCommulated, tree.EffStack);
                    var newTree = new TreeBranch { Pattern = tree.Pattern +" "+ accumulated, Pot = tree.Pot+ betSize, NodesThisRound = tree.NodesThisRound + 1, ActualBetSize = betSize, ActualCommulated = accumulated, EffStack = tree.EffStack };
                   output.AddRange(HandleRaise(newTree));
                }
            }

            return output;
        }

        private static List<TreeBranch> HandleRaise(TreeBranch tree)
        {
            var output = new List<TreeBranch>();
            //handle Call
            var pattern = tree.ActualCommulated < tree.EffStack
                ? tree.Pattern + " " + tree.ActualCommulated
                : tree.Pattern;

                            output.Add(new TreeBranch { Pattern = pattern, Pot = tree.Pot+tree.ActualBetSize, ActualBetSize = tree.ActualBetSize , ActualCommulated = tree.ActualCommulated, EffStack = tree.EffStack });


            //HandleRaise()
            if (tree.ActualCommulated < tree.EffStack)
            {
                var betArray = tree.Pattern.Split(" ");
                var previousStreet = betArray.Length > tree.NodesThisRound + 1
                    ? Int16.Parse(betArray[betArray.Length - tree.NodesThisRound-1])
                    : 0;
                var playerInvested = tree.NodesThisRound<2 ? 0 : int.Parse(betArray[betArray.Length - 2])-previousStreet;
                var raiseSize = (tree.ActualBetSize - playerInvested) *4;
                var betSize = raiseSize + playerInvested > tree.EffStack * 0.67m ? tree.EffStack - playerInvested : raiseSize;
                var accumulated = Math.Min(betSize + playerInvested, tree.EffStack);
              

                var newTree = new TreeBranch { Pattern = tree.Pattern + " " + accumulated, Pot = tree.Pot + betSize, NodesThisRound = tree.NodesThisRound + 1, ActualBetSize = betSize, ActualCommulated = accumulated, EffStack = tree.EffStack };
                output.AddRange(HandleRaise(newTree));
            }
            return output;
        }


        private class TreeBranch
        {
            public Decimal Pot { get; set; }
            public string Pattern { get; set; }
            public decimal ActualBetSize { get; set; }
            public decimal ActualCommulated { get; set; }
            public int NodesThisRound { get; set; }

            public decimal EffStack { get; set; }

        }

        public static void SaveScriptToFile( StringBuilder builder)
        {
            var contents = "";
            string path = @"C:\PioSolver\script.txt"; // path to file
            if (File.Exists(path))
            {
                contents = File.ReadAllText(path);

            }
            using FileStream fs = File.Create(path);
            byte[] info = new UTF8Encoding(true).GetBytes( builder.ToString());
            fs.Write(info, 0, info.Length);
        }


    }
}


