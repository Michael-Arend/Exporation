namespace Poker.Infrastructure.Models
{
    public struct Result
    {
        public Decimal Rating { get; set; }
        public ResultKind Kind { get; set; }
        public string Message { get; set; }


        public Result(IEnumerable<Card> cards)
        {
            Rating = new Decimal(0);
            Kind = new ResultKind();
            Message = string.Empty;


            var result = StraightFlush(cards);
            if (result.Rating > 0)
            {
                this = result;
                return;
            }

            result = FourOfAKind(cards);
            if (result.Rating > 0)
            {
                this = result;
                return;
            }

            result = FullHouse(cards);
            if (result.Rating > 0)
            {
                this = result;
                return;
            }

            result = Flush(cards);
            if (result.Rating > 0)
            {
                this = result;
                return;
            }

            result = Straight(cards);
            if (result.Rating > 0)
            {
                this = result;
                return;
            }

            result = ThreeOfKind(cards);
            if (result.Rating > 0)
            {
                this = result;
                return;
            }
            result = TwoPair(cards);
            if (result.Rating > 0)
            {
                this = result;
                return;
            }
            result = OnePair(cards);
            if (result.Rating > 0)
            {
                this = result;
                return;
            }

            this = HighCard(cards);

        }

        private Result HighCard(IEnumerable<Card> cards)
        {
            cards = cards.OrderByDescending(x => x.Value).Take(5).ToList();
            var rating = 0m;
            foreach (var card in cards)
            {
                rating += (decimal)card.Value / 1000;
            }
            return new Result(rating, ResultKind.HighCard, $"high card, {cards.First().ToString()}");
        }

        private Result(decimal rating, ResultKind kind, string message)
        {
            Rating = rating;
            Kind = kind;
            Message = message;
        }

        private Result StraightFlush(IEnumerable<Card> cards)
        {
            var straightResult = Straight(cards);
            if (Flush(cards).Rating > 0 && straightResult.Rating > 0)
            {
                return straightResult.Rating == 0.6m ? new Result(9, ResultKind.RoyalFlush, $"royal flush") : new Result(9, ResultKind.StraightFlush, straightResult.Message.Replace("straight", "straight flush"));
            }
            return new Result(0, ResultKind.HighCard, "");

        }

        private Result Flush(IEnumerable<Card> cards)
        {
            var colorGroups = cards.GroupBy(x => x.Color);
            var flushGroup = colorGroups.FirstOrDefault(x => x.Count() > 4);
            if (flushGroup == null) { return new Result(0, ResultKind.HighCard, ""); }
            var rating = 5m;
            foreach (var card in flushGroup)
            {
                rating += (decimal)card.Value / 1000;
            }
            var ordered = flushGroup.OrderByDescending(x => x.Value);
            var highest = ordered.First();
            return new Result(rating, ResultKind.Flush, $"straight,{highest.Value} high");

        }
        private Result Straight(IEnumerable<Card> cards)
        {
            cards = cards.OrderByDescending(x => x.Value).ToList();
            var highest = cards.First();
            var straightCount = 0;
            foreach (var card in cards)
            {
                if (card.Value == highest.Value - straightCount)
                {
                    straightCount++;
                }
                else
                {
                    straightCount = 0;
                    highest = card;
                }
                if (straightCount == 5)
                {
                    return new Result(4 + (decimal)highest.Value / 100, ResultKind.Straight, $"straight,{highest.Value} high");
                }
            }
            return new Result(0, ResultKind.HighCard, "");
        }


        private Result FullHouse(IEnumerable<Card> cards)
        {
            var oneList = cards.GroupBy(x => x.Value).Where(x => x.Count() == 3).OrderByDescending(x => x.Key).FirstOrDefault();
            if (oneList == null) { return new Result(0, ResultKind.HighCard, ""); }
            var second = cards.GroupBy(x => x.Value).Where(x => x.Key != oneList.Key).Where(x => x.Count() == 2).OrderByDescending(x => x.Key).FirstOrDefault();
            if (second == null) { return new Result(0, ResultKind.HighCard, ""); }
            var rating = 6 + (decimal)oneList.Key / 100 + (decimal)second.Key / 10000;
            return new Result(rating, ResultKind.FullHouse, $"full house,{oneList.Key}s over {oneList.Key}");
        }
        private Result FourOfAKind(IEnumerable<Card> cards)
        {
            var oneList = cards.GroupBy(x => x.Value).Where(x => x.Count() == 4).OrderByDescending(x => x.Key).FirstOrDefault();
            if (oneList ==null) { return new Result(0, ResultKind.HighCard, ""); }
            var rating = 7 + (decimal)oneList.Key / 100;
            cards.Where(x => x.Value != oneList.Key).OrderByDescending(x => x.Value).Take(1).Select(x => rating += (decimal)x.Value / 10000);
            return new Result(rating, ResultKind.FourOfAKind, $"four of a kind,{oneList.Key.ToString()}s");
        }

        private Result ThreeOfKind(IEnumerable<Card> cards)
        {
            var oneList = cards.GroupBy(x => x.Value).Where(x => x.Count() == 3).OrderByDescending(x => x.Key).FirstOrDefault();
            if (oneList == null) { return new Result(0, ResultKind.HighCard, ""); }
            var rating = 3 + (decimal)oneList.Key / 100;
            cards.Where(x => x.Value != oneList.Key).OrderByDescending(x => x.Value).Take(2).Select(x => rating += (decimal)x.Value / 10000);
            return new Result(rating, ResultKind.ThreeOfAKind, $"three of a kind,{oneList.Key.ToString()}s");
        }

        private Result TwoPair(IEnumerable<Card> cards)
        {
            var twoLists = cards.GroupBy(x => x.Value).Where(x => x.Count() == 2).OrderByDescending(x => x.Key).Take(2);
            if (twoLists == null || twoLists.Count() != 2) { return new Result(0, ResultKind.HighCard, ""); }
            var rating = 2 + (int)twoLists.First().Key / 100 + (int)twoLists.Last().Key / 100 + (decimal)cards.OrderByDescending(x => x.Value).First(x => !twoLists.Any(i => i.Key == x.Value)).Value / 10000;
            twoLists.First().Key.ToString();
            return new Result(rating, ResultKind.TwoPair, $"two pair,{twoLists.First().Key.ToString()}s and {twoLists.Last().Key.ToString()}s");
        }

        private Result OnePair(IEnumerable<Card> cards)
        {
            var oneList = cards.GroupBy(x => x.Value).Where(x => x.Count() == 2).OrderByDescending(x => x.Key).FirstOrDefault();
            if (oneList == null) { return new Result(0, ResultKind.HighCard, ""); }
            var rating = 1 + (decimal)oneList.Key / 100;
            cards.Where(x => x.Value != oneList.Key).OrderByDescending(x => x.Value).Take(3).Select(x => rating += (decimal)x.Value / 10000);
            return new Result(rating, ResultKind.Pair, $"one pair,{oneList.Key.ToString()}s");
        }
    }


    public enum ResultKind { RoyalFlush, StraightFlush, Flush, Straight, FullHouse, FourOfAKind, ThreeOfAKind, TwoPair, Pair, HighCard }
}
