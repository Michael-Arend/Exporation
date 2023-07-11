using PokerLibrary.Infrastructure.Enums;

namespace PokerLibrary.Infrastructure.Models;

public class Player
{
    public Hand Hand { get; set; }
    public string Name { get; set; }
    public Position Position { get; set; }
    public decimal Chips { get; set; }
    public decimal MoneyWon { get; set; }
    public bool PlayerInHand { get; set; } = true;

    public decimal ChipsInvestedInRound { get; set; }

    public Street MaxStreetReached { get; private set; } = Street.PreFlop;

    public HandRange ActualRange { get; set; }

    public void NextStreet()
    {
        MaxStreetReached++;
    }
}