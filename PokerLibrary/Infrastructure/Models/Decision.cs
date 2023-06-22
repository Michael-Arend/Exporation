﻿using PokerLibrary.Infrastructure.Enums;

namespace PokerLibrary.Infrastructure.Models;

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

