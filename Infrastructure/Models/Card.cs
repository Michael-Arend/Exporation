namespace Poker.Infrastructure.Models;

public class Card
{
    public Card(CardValue value, CardColor color)
    {
        Value = value;
        Color = color;
    }

    public CardValue Value { get; set; }
    public CardColor Color { get; set; }


    public string GetStringFromCard()
    {
        var output = "";
        switch (Value)
        {
            case CardValue.Two:
                output = "2";
                break;
            case CardValue.Three:
                output = "3";
                break;
            case CardValue.Four:
                output = "4";
                break;
            case CardValue.Five:
                output = "5";
                break;
            case CardValue.Six:
                output = "6";
                break;
            case CardValue.Seven:
                output = "7";
                break;
            case CardValue.Eight:
                output = "8";
                break;
            case CardValue.Nine:
                output = "9";
                break;
            case CardValue.Ten:
                output = "T";
                break;
            case CardValue.Jack:
                output = "J";
                break;
            case CardValue.Queen:
                output = "Q";
                break;
            case CardValue.King:
                output = "K";
                break;
            case CardValue.Ace:
                output = "A";
                break;
        }
        switch (Color)
        {
            case CardColor.C:
                output += "c";
                break;
            case CardColor.D:
                output += "d";
                break;
            case CardColor.S:
                output += "s";
                break;
            case CardColor.H:
                output += "h";
                break;
        }

        return output;
    }


    public static Card GetCardFromString(string cardString)
    {
        var value = new CardValue();
        switch (cardString.Substring(0, 1))
        {
            case "2":
                value = CardValue.Two;
                break;
            case "3":
                value = CardValue.Three;
                break;
            case "4":
                value = CardValue.Four;
                break;
            case "5":
                value = CardValue.Five;
                break;
            case "6":
                value = CardValue.Six;
                break;
            case "7":
                value = CardValue.Seven;
                break;
            case "8":
                value = CardValue.Eight;
                break;
            case "9":
                value = CardValue.Nine;
                break;
            case "T":
                value = CardValue.Ten;
                break;
            case "J":
                value = CardValue.Jack;
                break;
            case "Q":
                value = CardValue.Queen;
                break;
            case "K":
                value = CardValue.King;
                break;
            case "A":
                value = CardValue.Ace;
                break;
            default:
                break;
        }

        var color = new CardColor();
        switch (cardString.Substring(1, 1))
        {
            case "h":
                color = CardColor.H;
                break;
            case "s":
                color = CardColor.S;
                break;
            case "d":
                color = CardColor.D;
                break;
            case "c":
                color = CardColor.C;
                break;

        }

        return new Card ( value, color);
    }
}

public enum CardValue
{
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13,
    Ace = 14
}

public enum CardColor
{
    S = 1,
    C = 2,
    D = 3,
    H = 4,
  
  
}