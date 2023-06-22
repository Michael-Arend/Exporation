namespace PokerLibrary.Infrastructure.Models;

public struct Result
{
    public decimal Rating { get; set; }
    public ResultKind Kind { get; set; }
    public string Message { get; set; }


    public Result(decimal rating, ResultKind kind, string message)
    {
        Rating = rating;
        Kind = kind;
        Message = message;
    }
}

public enum ResultKind
{
    RoyalFlush,
    StraightFlush,
    Flush,
    Straight,
    FullHouse,
    FourOfAKind,
    ThreeOfAKind,
    TwoPair,
    Pair,
    HighCard
}