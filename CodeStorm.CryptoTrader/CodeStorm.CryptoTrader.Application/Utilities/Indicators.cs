namespace CodeStorm.CryptoTrader.Application.Utilities
{
    public static class Indicators
    {
        public static List<decimal?> CalculateRSI(List<decimal> prices, int period = 14)
        {
            var rsi = new List<decimal?>();

            if (prices.Count < period + 1)
                return prices.Select(x => (decimal?)null).ToList();

            decimal gain = 0, loss = 0;

            // Initial average
            for (int i = 1; i <= period; i++)
            {
                var delta = prices[i] - prices[i - 1];
                if (delta > 0) gain += delta;
                else loss -= delta;
            }

            gain /= period;
            loss /= period;

            decimal rs = loss == 0 ? 100 : gain / loss;
            rsi.AddRange(Enumerable.Repeat<decimal?>(null, period));
            rsi.Add(100 - (100 / (1 + rs)));

            // Wilder smoothing
            for (int i = period + 1; i < prices.Count; i++)
            {
                var delta = prices[i] - prices[i - 1];
                var currentGain = delta > 0 ? delta : 0;
                var currentLoss = delta < 0 ? -delta : 0;

                gain = ((gain * (period - 1)) + currentGain) / period;
                loss = ((loss * (period - 1)) + currentLoss) / period;

                rs = loss == 0 ? 100 : gain / loss;
                rsi.Add(100 - (100 / (1 + rs)));
            }

            return rsi;
        }

        public static decimal? CalculateLatestRSI(List<decimal> prices, int period = 14)
        {
            if (prices.Count < period + 1)
                return null; // not enough data

            decimal gainSum = 0, lossSum = 0;

            for (int i = 1; i <= period; i++)
            {
                decimal change = prices[i] - prices[i - 1];
                gainSum += Math.Max(0, change);
                lossSum += Math.Max(0, -change);
            }

            decimal avgGain = gainSum / period;
            decimal avgLoss = lossSum / period;

            for (int i = period + 1; i < prices.Count; i++)
            {
                decimal change = prices[i] - prices[i - 1];
                decimal gain = Math.Max(0, change);
                decimal loss = Math.Max(0, -change);

                avgGain = ((avgGain * (period - 1)) + gain) / period;
                avgLoss = ((avgLoss * (period - 1)) + loss) / period;
            }

            decimal rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
            return 100 - (100 / (1 + rs));
        }

        public static decimal? CalculateLatestRSIWithWilderSmoothing(List<decimal> prices, int period = 14)
        {
            if (prices == null || prices.Count <= period)
                return null;

            decimal gain = 0, loss = 0;

            // Step 1: calculate initial average gain/loss
            for (int i = 1; i <= period; i++)
            {
                var change = prices[i] - prices[i - 1];
                if (change > 0) gain += change;
                else loss -= change;
            }

            gain /= period;
            loss /= period;

            decimal rs = loss == 0 ? 100 : gain / loss;
            decimal rsi = 100 - (100 / (1 + rs));

            // Step 2: apply smoothing for the rest of the values
            for (int i = period + 1; i < prices.Count; i++)
            {
                var change = prices[i] - prices[i - 1];
                var currentGain = change > 0 ? change : 0;
                var currentLoss = change < 0 ? -change : 0;

                gain = ((gain * (period - 1)) + currentGain) / period;
                loss = ((loss * (period - 1)) + currentLoss) / period;

                rs = loss == 0 ? 100 : gain / loss;
                rsi = 100 - (100 / (1 + rs));
            }

            return Math.Round(rsi, 2); // return only final RSI like UI shows
        }

        public static List<decimal?> CalculateStochRSI(List<decimal?> rsi, int stochPeriod = 14, int kSmoothing = 3, int dSmoothing = 3)
        {
            var stochRsi = new List<decimal?>();
            var percentK = new List<decimal?>();
            var percentD = new List<decimal?>();

            for (int i = 0; i < rsi.Count; i++)
            {
                if (i < stochPeriod || rsi[i] == null)
                {
                    stochRsi.Add(null);
                    continue;
                }

                var window = rsi.Skip(i - stochPeriod + 1).Take(stochPeriod).Where(x => x.HasValue).Select(x => x.Value).ToList();
                decimal current = rsi[i].Value;
                decimal min = window.Min();
                decimal max = window.Max();

                decimal value = max == min ? 0 : (current - min) / (max - min);
                stochRsi.Add(value);
            }

            // Smooth %K
            for (int i = 0; i < stochRsi.Count; i++)
            {
                if (i < kSmoothing - 1 || stochRsi[i] == null)
                {
                    percentK.Add(null);
                    continue;
                }

                var window = stochRsi.Skip(i - kSmoothing + 1).Take(kSmoothing).Where(x => x.HasValue).Select(x => x.Value).ToList();
                percentK.Add(window.Average());
            }

            // Smooth %D
            for (int i = 0; i < percentK.Count; i++)
            {
                if (i < dSmoothing - 1 || percentK[i] == null)
                {
                    percentD.Add(null);
                    continue;
                }

                var window = percentK.Skip(i - dSmoothing + 1).Take(dSmoothing).Where(x => x.HasValue).Select(x => x.Value).ToList();
                percentD.Add(window.Average());
            }

            // Print latest values like TradingView
            Console.WriteLine($"%K (blue): {percentK.LastOrDefault() * 100:F2}");
            Console.WriteLine($"%D (orange): {percentD.LastOrDefault() * 100:F2}");

            return percentK; // Or return a tuple of %K and %D if you want
        }

    }

    public class StochRsiResult
    {
        public decimal? StochRsi { get; set; }
        public decimal? PercentK { get; set; } // Smoothed %K
        public decimal? PercentD { get; set; } // %D (SMA of %K)
    }   
}
