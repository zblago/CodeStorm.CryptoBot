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

        public static Tuple<decimal?, decimal?> CalculateStochRSI(List<decimal?> rsi, int stochPeriod = 14, int kSmoothing = 3, int dSmoothing = 3)
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

            return new Tuple<decimal?, decimal?> (percentK.LastOrDefault() * 100, percentD.LastOrDefault() * 100); // Or return a tuple of %K and %D if you want
        }

        public static List<decimal?> CalculateEMA(List<decimal> prices, int period)
        {
            var ema = new List<decimal?>();
            decimal multiplier = 2m / (period + 1);
            decimal? previousEma = null;

            for (int i = 0; i < prices.Count; i++)
            {
                if (i < period - 1)
                {
                    ema.Add(null);
                    continue;
                }
                if (i == period - 1)
                {
                    decimal sma = prices.GetRange(0, period).Average();
                    ema.Add(sma);
                    previousEma = sma;
                }
                else
                {
                    decimal newEma = ((prices[i] - previousEma.Value) * multiplier) + previousEma.Value;
                    ema.Add(newEma);
                    previousEma = newEma;
                }
            }

            return ema;
        }

        public static (List<decimal?> stochK, List<decimal?> stochD) CalculateStochasticRSI(List<decimal> closes, int rsiPeriod = 14, int stochPeriod = 14, int kSmooth = 3, int dSmooth = 3)
        {
            var rsi = CalculateRSI(closes, rsiPeriod);
            var stochK = new List<decimal?>();
            var stochD = new List<decimal?>();

            for (int i = 0; i < rsi.Count; i++)
            {
                if (i < stochPeriod + rsiPeriod - 1 || !rsi[i].HasValue)
                {
                    stochK.Add(null);
                    continue;
                }

                var rsiSlice = rsi.Skip(i - stochPeriod + 1).Take(stochPeriod).Where(v => v.HasValue).Select(v => v.Value).ToList();
                decimal minRsi = rsiSlice.Min();
                decimal maxRsi = rsiSlice.Max();

                decimal k = (maxRsi - minRsi == 0) ? 0 : (rsi[i].Value - minRsi) / (maxRsi - minRsi) * 100;
                stochK.Add(k);
            }

            // Smooth %K
            var smoothedK = Smooth(stochK, kSmooth);
            var smoothedD = Smooth(smoothedK, dSmooth);

            return (smoothedK, smoothedD);
        }

        // Calculates Simple Moving Average (SMA) smoothing
        private static List<decimal?> Smooth(List<decimal?> input, int period)
        {
            var result = new List<decimal?>();

            for (int i = 0; i < input.Count; i++)
            {
                if (i < period - 1 || input.Skip(i - period + 1).Take(period).Any(v => !v.HasValue))
                {
                    result.Add(null);
                    continue;
                }

                decimal avg = input.Skip(i - period + 1).Take(period).Select(v => v.Value).Average();
                result.Add(avg);
            }

            return result;
        }
    }

    public class StochRsiResult
    {
        public decimal? StochRsi { get; set; }
        public decimal? PercentK { get; set; } // Smoothed %K
        public decimal? PercentD { get; set; } // %D (SMA of %K)
    }   
}
