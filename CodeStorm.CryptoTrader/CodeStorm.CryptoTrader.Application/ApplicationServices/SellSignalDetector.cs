namespace CodeStorm.CryptoTrader.Application.ApplicationServices
{
    public class SellSignalDetector
    {
        public static bool IsSellSignal(
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

            // Safety null check
            if (!rsi[last].HasValue || !stochK[last].HasValue || !stochK[last - 1].HasValue)
                return false;

            // Indicator values
            decimal currentRSI = rsi[last].Value;
            decimal currentStochK = stochK[last].Value;
            decimal prevStochK = stochK[last - 1].Value;

            decimal currentEma9 = ema9[last];
            decimal currentEma21 = ema21[last];

            // Sell signal logic:
            bool rsiWeak = currentRSI < 50;
            bool stochKOverboughtCrossDown = prevStochK > 80 && currentStochK < 80;
            bool emaBearish = currentEma9 < currentEma21;

            return rsiWeak && stochKOverboughtCrossDown && emaBearish;
        }
    }
}