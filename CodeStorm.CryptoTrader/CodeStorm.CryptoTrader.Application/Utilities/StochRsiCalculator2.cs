namespace CodeStorm.CryptoTrader.Application.Utilities
{
    public static class StochRsiCalculator2
    {
        public class StochasticRsiResult
        {
            public decimal? Rsi { get; set; }
            public decimal? PercentK { get; set; }
            public decimal? PercentD { get; set; }
        }

        public static List<StochasticRsiResult> CalculateStochasticRsi(
            List<decimal> closes,
            int rsiPeriod = 14,
            int stochPeriod = 14,
            int kSmoothing = 3,
            int dSmoothing = 3)
        {
            int totalPeriod = rsiPeriod + stochPeriod + dSmoothing;
            var rsi = CalculateRsi(closes, rsiPeriod);

            List<decimal?> stochRsi = new List<decimal?>();
            for (int i = 0; i < closes.Count; i++)
            {
                if (i < rsiPeriod + stochPeriod - 1)
                {
                    stochRsi.Add(null);
                    continue;
                }

                var rsiSlice = rsi.Skip(i - stochPeriod + 1).Take(stochPeriod).ToList();
                if (rsiSlice.Any(r => r == null))
                {
                    stochRsi.Add(null);
                    continue;
                }

                var minRsi = rsiSlice.Min().Value;
                var maxRsi = rsiSlice.Max().Value;
                var currentRsi = rsi[i];

                decimal value = (maxRsi - minRsi == 0) ? 0 : ((currentRsi.Value - minRsi) / (maxRsi - minRsi));
                stochRsi.Add(value * 100);
            }

            // Smooth %K (3-period SMA)
            List<decimal?> percentK = Smooth(stochRsi, kSmoothing);

            // Smooth %D (3-period SMA of %K)
            List<decimal?> percentD = Smooth(percentK, dSmoothing);

            var result = new List<StochasticRsiResult>();
            for (int i = 0; i < closes.Count; i++)
            {
                result.Add(new StochasticRsiResult
                {
                    Rsi = rsi[i],
                    PercentK = percentK[i],
                    PercentD = percentD[i]
                });
            }

            return result;
        }

        public static List<decimal?> CalculateRsi(List<decimal> closes, int period)
        {
            List<decimal?> rsi = new List<decimal?>();
            List<decimal> gains = new List<decimal>();
            List<decimal> losses = new List<decimal>();

            rsi.AddRange(Enumerable.Repeat<decimal?>(null, period));

            for (int i = 1; i <= period; i++)
            {
                decimal change = closes[i] - closes[i - 1];
                gains.Add(Math.Max(change, 0));
                losses.Add(Math.Max(-change, 0));
            }

            decimal avgGain = gains.Average();
            decimal avgLoss = losses.Average();

            decimal rs = (avgLoss == 0) ? 100 : avgGain / avgLoss;
            rsi.Add(100 - (100 / (1 + rs)));

            for (int i = period + 1; i < closes.Count; i++)
            {
                decimal change = closes[i] - closes[i - 1];
                decimal gain = Math.Max(change, 0);
                decimal loss = Math.Max(-change, 0);

                avgGain = (avgGain * (period - 1) + gain) / period;
                avgLoss = (avgLoss * (period - 1) + loss) / period;

                rs = (avgLoss == 0) ? 100 : avgGain / avgLoss;
                rsi.Add(100 - (100 / (1 + rs)));
            }

            return rsi;
        }

        public static List<decimal?> Smooth(List<decimal?> values, int period)
        {
            var result = new List<decimal?>();
            for (int i = 0; i < values.Count; i++)
            {
                if (i < period - 1 || values.Skip(i - period + 1).Take(period).Any(v => v == null))
                {
                    result.Add(null);
                }
                else
                {
                    result.Add(values.Skip(i - period + 1).Take(period).Average().Value);
                }
            }
            return result;
        }
    }
}
