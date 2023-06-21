using Poker.Pio.Connection;

namespace Poker.Pio.Util;

public static class StrategyUtil
{
    public static List<string> GetOptions(SolverConnection solver, string nodeString)
    {
        var response = solver.GetResponseFromSolver("show_children " + nodeString);

        var options = response.ToList().Where(x => x.Contains(nodeString)).Select(x => x.Replace(nodeString + ":", ""))
            .ToList();

        return options;
    }

    public static List<String> GetStrategies(SolverConnection solver, string nodeString)
    {
        var result = solver.GetResponseFromSolver("show_strategy_pp " + nodeString).ToList();
        return result;

    }
    



}