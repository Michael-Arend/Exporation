using Client.Util;
using Poker.Pio.Connection;
using PokerLibrary.Business;

namespace Client.Plugins
{

    /// <summary>
    /// - load a file, rebuild forgotten streets, save as full save
    /// </summary>
    public class FileRebuilder
    {
        private string TreePath { get; set; }

        private SolverConnection _solver { get; set; }


        public FileRebuilder(Dictionary<string, string> arguments)
        {
            // start solver executable
            _solver = new SolverConnection(arguments["-solver"]);
            TreePath = arguments["-tree"];
        }


        public void Run()
        {
            TreeUtil.LoadTree(_solver, TreePath);
            if (!File.Exists(TreePath))
            {
                GameBusinessHandler.SendMessage("Specified tree file doesn't exist.");
                return;
            }
            if (TreeUtil.GetCalculatedBoardLengthInLoadedTree(_solver, TreePath) == 5)
            {
                GameBusinessHandler.SendMessage($"Tree {TreePath} is already a full save!");
                return;
            }

            TreeUtil.RebuildForgottenStreets(_solver, TreePath);
            TreeUtil.DumpTree(_solver, TreePath, TreeUtil.TreeSize.full);
        }
    }
}
