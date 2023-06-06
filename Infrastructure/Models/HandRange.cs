namespace Poker.Infrastructure.Models
{
    public class HandRange
    {
        public HandRange()
        {

        }
        public HandRange(string bettingPattern, decimal size, string? path, string ranges)
        {
            RangesText = ranges;
            Frequencies = ranges.Substring(0, 1) == "[" ? CreateFrequenciesForCarrots(ranges) : CreateFrequenciesForGtoWizard(ranges);
            BetSize = size;
            BettingPattern = bettingPattern;
            Path = path;
        }

        public decimal BetSize { get; set; }
        public string BettingPattern { get; set; }

        public string RangesText { get; set; }

        public string? Path { get; set; }

        public List<Frequency> Frequencies { get; set; }




        private List<Frequency> CreateFrequenciesForCarrots(string ranges)
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
                var cut = value == 1 ? 0 : value > 0.1m ? 5 : 6;
                var cards = group.Substring(cut, group.Length - cut * 2).Split(", ");
                foreach (var card in cards)
                {
                    var frequency = new Frequency(new Hand(card), value);
                    output.Add(frequency);
                }
            }

            return output;
        }

        List<Frequency> CreateFrequenciesForGtoWizard(string ranges)
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