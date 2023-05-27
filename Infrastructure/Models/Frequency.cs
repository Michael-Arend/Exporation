namespace Poker.Infrastructure.Models;

public class Frequency
{
    public Frequency(Hand hand, decimal value)
    {
        Hand = hand;
        Value = value;
    }

    public Hand Hand { get; set; }
    public decimal Value { get; set; }
}