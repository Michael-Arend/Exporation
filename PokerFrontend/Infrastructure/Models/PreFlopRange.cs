using System;
using System.Collections.Generic;

namespace PokerFrontend.Infrastructure.Models;

public class PreFlopRange
{
    public PreFlopRange(string name, int startingStacks, int limit, double rakeInPercent, double rakeCapDollar, IEnumerable<PreFlopAction> preFlopActions)
    {
        Id = Guid.NewGuid();
        Name = name;
        StartingStacks = startingStacks;
        Limit = limit;
        RakeInPercent = rakeInPercent;
        RakeCapDollar = rakeCapDollar;
        PreFlopActions = preFlopActions;
    }
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int StartingStacks { get; set; }
    public int Limit { get; set; }
    public double RakeInPercent { get; set; }
    public double RakeCapDollar { get; set; }

    public IEnumerable<PreFlopAction> PreFlopActions { get; set; }
}