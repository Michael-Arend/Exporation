using Poker.Infrastructure.Enums;

namespace Poker.Infrastructure.Models;

public class Decision
{
    public Decision(DecisionKind kind, decimal amount)
    {
        Kind = kind;
        Amount = amount;
    }

    public DecisionKind Kind { get; set; }
    public decimal Amount { get; set; }
}

