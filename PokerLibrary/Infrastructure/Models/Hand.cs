using PokerLibrary.Infrastructure.Enums;

namespace PokerLibrary.Infrastructure.Models
{
    public class Hand
    {
        public Hand(Card card1, Card card2)
        {
            Card1 = card1;
            Card2 = card2;
        }
        public Hand(string handString)
        {
            Card1 = Card.GetCardFromString(handString.Substring(0, 2));
            Card2 = Card.GetCardFromString(handString.Substring(2, 2));
        }

        public string GetStringFromHand()
        {
            return $"{Card1.GetStringFromCard()} {Card2.GetStringFromCard()}";
        }




        public string GetSortedStringFromHand()
        {
            if (Card1.Value == Card2.Value)
            {
                return (int)Card1.Color < (int)Card2.Color ? $"{Card1.GetStringFromCard()}{Card2.GetStringFromCard()}"
                    : $"{Card2.GetStringFromCard()}{Card1.GetStringFromCard()}";
            }

            return Card1.Value > Card2.Value
                ? $"{Card1.GetStringFromCard()}{Card2.GetStringFromCard()}"
                : $"{Card2.GetStringFromCard()}{Card1.GetStringFromCard()}";

        }

        public Hand Convert(Dictionary<CardColor, CardColor> solverConversion)
        {
            var c1Color = solverConversion.GetValueOrDefault(Card1.Color);
            var c2Color = solverConversion.GetValueOrDefault(Card2.Color);

            var card1 = new Card(Card1.Value, c1Color == 0 ? Card1.Color : c1Color);
            var card2 = new Card(Card2.Value, c2Color == 0 ? Card2.Color : c2Color);
            return new Hand(card1, card2);
        }

        public Card Card1 { get; set; }
        public Card Card2 { get; set; }

    }
}
