using PokerLibrary.Pio.Util;

namespace PokerLibrary.Infrastructure.Models
{
    public class HandRange
    {
      
        public HandRange(string bettingPattern, decimal size, string? path, string range)
        {
            RangeText = range;
            Frequencies = CreateFrequencies(range);
            BetSize = size;
            BettingPattern = bettingPattern;
            Path = path;
        }


        public decimal BetSize { get; set; }
        public string BettingPattern { get; set; }

        public string RangeText { get; set; }

        public string? Path { get; set; }

        public IEnumerable<Frequency> Frequencies { get; set; }




        private static IEnumerable<Frequency> CreateFrequencies(string range)
        {
            if (range[..1] == "[")
            {
                return CreateFrequenciesForCarrots(range);
            }

            return range.Split(" ").Length == 1326 ? CreateFrequenciesForPioStandard(range) : CreateFrequenciesForGtoWizard(range);
        }

        private static IEnumerable<Frequency> CreateFrequenciesForPioStandard(string range)
        {
            var result = new List<Frequency>();
            var r = range.Split(" ");

            var handOrder = HandOrder.GetHandOrderArray();
            var i=0;
            foreach (var hand in handOrder)
            {
                result.Add(new Frequency(new Hand(hand),decimal.Parse(r[i])));
                i++;
            }

            return result;
        }



        private static IEnumerable<Frequency> CreateFrequenciesForCarrots(string ranges)
        {
            var output = new List<Frequency>();

            var groups = ranges.Split("], ");

            foreach (var group in groups)
            {

                var valueString = group.Substring(0, 1) != "["
                    ? "100"
                    : @group.Substring(5, 1) == "]" ? @group.Substring(1, 4) : @group.Substring(1, 5);

                var value = Decimal.Parse(valueString);
                value /= 100;
                var cut = value == 1 ? 0 : value > 10 ? 7 : 6;
                var cards = group.Substring(cut, group.Length - cut * 2).Split(", ");
                foreach (var card in cards)
                {
                    var frequency = new Frequency(new Hand(card), value);
                    output.Add(frequency);
                }
            }

            return output;
        }

        static IEnumerable<Frequency> CreateFrequenciesForGtoWizard(string ranges)
        {
            var output = new List<Frequency>();

            var hands = ranges.Split(",");

            foreach (var hand in hands)
            {
                var arr = hand.Split(':');
                var cards = arr[0];
                var value = decimal.Parse(arr[1]);
                var newFrequency = new Frequency(new Hand(cards), value);
                output.Add(newFrequency);
            }

            return output;
        }
    }
}