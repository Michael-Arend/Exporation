using DecisionKind = PokerFrontend.Infrastructure.Enums.DecisionKind;

namespace PokerFrontend.Infrastructure.Models;

public class PreFlopAction
{
    public PreFlopAction(string name, string pattern, DecisionKind decision, decimal betSizing, string folder, string range, bool isDefault = true)
    {
        Name = name;
        Pattern = pattern;
        Decision = decision;
        BetSizing = betSizing;
        Folder = folder;
        Range = range;
        IsDefault = isDefault;
    }

    public string Name { get; set; }
    public string Pattern { get; set; }
    public DecisionKind Decision { get; set; }
    public decimal BetSizing { get; set; }
    public string Folder { get; set; }
    public string Range { get; set; }
    public bool IsDefault { get; set; }
}