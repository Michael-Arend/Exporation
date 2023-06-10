using Poker.Infrastructure.Enums;
using Poker.Infrastructure.Models;

namespace Poker.Infrastructure.Helper.Extensions
{
    public static class CardColorExtensions
    {
        public static char GetCharFromColor(this CardColor color)
        {
            switch (color)
            {
                case CardColor.S:
                    return 's';
                case CardColor.H:
                    return 'h';
                case CardColor.C:
                    return 'c';
                case CardColor.D:
                    return 'd';
            }
            return 'd';

        }
    }
}


