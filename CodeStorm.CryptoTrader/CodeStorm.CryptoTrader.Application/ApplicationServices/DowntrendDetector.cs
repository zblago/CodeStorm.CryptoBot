namespace CodeStorm.CryptoTrader.Application.ApplicationServices
{
    using CodeStorm.CryptoTrader.Application.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DowntrendDetector
    {
        public static int FastEmaPeriod { get; set; } = 9;
        public static int SlowEmaPeriod { get; set; } = 21;
        public static int RsiPeriod { get; set; } = 14;

        public static bool IsDowntrend(List<decimal> closePrices)
        {
            if (closePrices == null || closePrices.Count < Math.Max(SlowEmaPeriod, RsiPeriod) + 1)
                return false;

            var fastEma = Indicators.CalculateEMA(closePrices, FastEmaPeriod);
            var slowEma = Indicators.CalculateEMA(closePrices, SlowEmaPeriod);
            var rsi = Indicators.CalculateRSI(closePrices.ToList());

            int i = closePrices.Count - 1;
            bool emaCross = fastEma[i] < slowEma[i] && fastEma[i - 1] >= slowEma[i - 1];
            bool rsiFalling = rsi[i] < 50 && rsi[i] < rsi[i - 1];

            return emaCross && rsiFalling;
        }        
    }
}
