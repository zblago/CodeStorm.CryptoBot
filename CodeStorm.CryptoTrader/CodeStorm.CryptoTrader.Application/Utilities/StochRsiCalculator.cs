namespace CodeStorm.CryptoTrader.Application.Utilities
{
    public static class StochRsiCalculator
    {
        // Step 1: Calculate RSI using Wilder's method
        public static List<decimal?> CalculateRsi(List<decimal> closes, int period = 14)
        {
            var rsi = new List<decimal?>();
            decimal gain = 0, loss = 0;

            for (int i = 1; i <= period; i++)
            {
                var delta = closes[i] - closes[i - 1];
                if (delta > 0) gain += delta;
                else loss -= delta;
            }

            gain /= period;
            loss /= period;

            rsi.AddRange(Enumerable.Repeat<decimal?>(null, period));
            rsi.Add(100 - (100 / (1 + gain / loss)));

            for (int i = period + 1; i < closes.Count; i++)
            {
                var delta = closes[i] - closes[i - 1];
                decimal currentGain = delta > 0 ? delta : 0;
                decimal currentLoss = delta < 0 ? -delta : 0;

                gain = (gain * (period - 1) + currentGain) / period;
                loss = (loss * (period - 1) + currentLoss) / period;

                rsi.Add(loss == 0 ? 100 : 100 - (100 / (1 + gain / loss)));
            }

            return rsi;
        }

        // Step 2–4: Calculate StochRSI, %K, and %D
        public static List<StochRsiResult> CalculateStochRsi(
            List<decimal> closes,
            int rsiPeriod = 14,
            int stochPeriod = 14,
            int smoothK = 3,
            int smoothD = 3)
        {
            var rsi = CalculateRsi(closes, rsiPeriod);
            var stochRsi = new List<decimal?>();
            var percentK = new List<decimal?>();
            var percentD = new List<decimal?>();
            var results = new List<StochRsiResult>();

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

                stochRsi.Add(max == min ? 0 : (current - min) / (max - min));
            }

            for (int i = 0; i < stochRsi.Count; i++)
            {
                if (i < smoothK - 1 || stochRsi[i] == null)
                {
                    percentK.Add(null);
                    continue;
                }

                var kWindow = stochRsi.Skip(i - smoothK + 1).Take(smoothK).Where(x => x.HasValue).Select(x => x.Value).ToList();
                percentK.Add(kWindow.Average());
            }

            for (int i = 0; i < percentK.Count; i++)
            {
                if (i < smoothD - 1 || percentK[i] == null)
                {
                    percentD.Add(null);
                    results.Add(new StochRsiResult { PercentK = null, PercentD = null });
                    continue;
                }

                var dWindow = percentK.Skip(i - smoothD + 1).Take(smoothD).Where(x => x.HasValue).Select(x => x.Value).ToList();
                results.Add(new StochRsiResult
                {
                    PercentK = percentK[i] * 100,
                    PercentD = dWindow.Average() * 100
                });
            }

            return results;
        }
    }

}
