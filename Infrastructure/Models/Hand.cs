using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker.Infrastructure.Models
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
            Card1 = Card.GetCardFromString(handString.Substring(0,2));
            Card2 = Card.GetCardFromString(handString.Substring(2, 2));
        }

        public String GetStringFromHand()
        {
            return $"{Card1.GetStringFromCard()}{Card2.GetStringFromCard()}";
        }

        public String GetSortedStringFromHand()
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




        public Card Card1 { get; set; }
        public Card Card2 { get; set; }

    }
}
