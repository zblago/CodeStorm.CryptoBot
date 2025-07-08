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
            if (!rsi[last].HasValue || !rsi[last - 1].HasValue || !stochK[last].HasValue || !stochK[last - 1].HasValue)
                return false;

            // Indicator values
            decimal currentRSI = rsi[last].Value;
            decimal prevRSI = rsi[last - 1].Value;
            decimal currentStochK = stochK[last].Value;
            decimal prevStochK = stochK[last - 1].Value;

            decimal currentEma9 = ema9[last];
            decimal currentEma21 = ema21[last];

            // Sell signal logic:
            bool stochCrossDown = prevStochK > 80 && currentStochK < 80;
            bool rsiFalling = currentRSI < prevRSI;
            bool emaBearish = currentEma9 < currentEma21;

            return stochCrossDown;// && rsiFalling;// && emaBearish;
        }

        public static (decimal currentEma9, decimal currentEma21, decimal currentRSI, decimal prevRSI) 
            GetIndicators(List<decimal> closes, List<decimal> ema9, List<decimal> ema21,List<decimal?> rsi)
        {
            int last = closes.Count - 1;

            decimal currentEma9 = ema9[last];
            decimal currentEma21 = ema21[last];
            decimal currentRSI = rsi[last].Value;
            decimal prevRSI = rsi[last - 1].Value;

            return (currentEma9, currentEma21, currentRSI, prevRSI);
        }
    }
}