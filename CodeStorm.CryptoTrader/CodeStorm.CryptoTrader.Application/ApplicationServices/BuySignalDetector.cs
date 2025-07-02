namespace CodeStorm.CryptoTrader.Application.ApplicationServices
{
    public class BuySignalDetector
    {
        public static bool IsBuySignal(
            List<decimal> closePrices,
            List<decimal> ema9,
            List<decimal> ema21,
            List<decimal?> rsi14,
            List<decimal?> stochRsiK,
            List<decimal?> stochRsiD)
        {
            int i = closePrices.Count - 1;

            // Basic sanity checks
            if (i < 1 ||
                ema9.Count <= i ||
                ema21.Count <= i ||
                rsi14.Count <= i ||
                stochRsiK.Count <= i ||
                stochRsiD.Count <= i)
                return false;

            // Trend filter: EMA 9 > EMA 21
            bool isUptrend = ema9[i] > ema21[i];

            // RSI filter: RSI > 50
            bool rsiHealthy = rsi14[i].HasValue && rsi14[i] > 50;

            // Stoch RSI cross from below 20
            bool stochRsiCrossUp =
                stochRsiK[i - 1] < stochRsiD[i - 1] &&
                stochRsiK[i] > stochRsiD[i] &&
                stochRsiK[i - 1] < 20 && stochRsiD[i - 1] < 20;

            return isUptrend && rsiHealthy && stochRsiCrossUp;
        }
    }

}
