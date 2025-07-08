namespace CodeStorm.CryptoTrader.Application.ApplicationServices
{
    public class BuySignalDetector
    {
        public static bool IsBuySignal(
            List<decimal> closes,
            List<decimal> ema9,
            List<decimal> ema21,
            List<decimal?> rsi,
            List<decimal?> stochK,
            List<decimal?> stochD)
        {
            int last = closes.Count - 1;
            if (last < 1 || ema9.Count != closes.Count || ema21.Count != closes.Count)
                return false;

            // Safety checks
            if (!rsi[last].HasValue || !rsi[last - 1].HasValue || !stochK[last].HasValue || !stochK[last - 1].HasValue)
                return false;

            decimal currentRSI = rsi[last].Value;
            decimal prevRSI = rsi[last - 1].Value;
            decimal currentStochK = stochK[last].Value;
            decimal prevStochK = stochK[last - 1].Value;
            decimal currentEma9 = ema9[last];
            decimal currentEma21 = ema21[last];

            // Updated conditions:
            bool stochCrossUp = prevStochK < 20 && currentStochK > 20;
            bool rsiRising = currentRSI > prevRSI;
            bool emaUpTrend = currentEma9 > currentEma21;

            return stochCrossUp;// && rsiRising;// && emaUpTrend;
        }
    }
}
