using CodeStorm.CryptoTrader.Application.Utilities;
using CodeStorm.CryptoTrader.Repository.Repository;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.Json;

namespace CodeStorm.CryptoTrader.Application.ApplicationServices
{
    public class RealTimeIndicatorService
    {
        private readonly ResponseRepository _responseRepository;
        private readonly TimelineAnalysisRepository _timelineAnalysisRepository;
        private readonly ActionSignalRepository _actionSignalRepository;
        private readonly ExecutedActionRepository _executedActionRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        public RealTimeIndicatorService(ResponseRepository responseRepository,
            TimelineAnalysisRepository timelineAnalysisRepository,
            ActionSignalRepository actionSignalRepository,
            ExecutedActionRepository executedActionRepository,
            IHttpClientFactory httpClientFactory) 
        { 
            _responseRepository = responseRepository;
            _httpClientFactory = httpClientFactory;
            _timelineAnalysisRepository = timelineAnalysisRepository;
            _actionSignalRepository = actionSignalRepository;
            _executedActionRepository = executedActionRepository;
        }

        public async Task GetLatestOHLCForFwog()
        {
            var httpRequestMessage = new HttpRequestMessage
                (HttpMethod.Get, $"https://api.kraken.com/0/public/OHLC?pair={CryptoCurrencyType.FWOGUSD }&interval=15");

            var httpClient = _httpClientFactory.CreateClient();
            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var jsonResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                var ohlcResponse = JsonConvert.DeserializeObject<KrakenResponse>(jsonResponse);

                if (ohlcResponse != null && ohlcResponse.Result != null)
                {
                    var lastClosedIndex = ohlcResponse.Result.Last;
                    var closePrices = new List<decimal>();
                    var lastCloseOhlcBasedOnIndex = ohlcResponse
                        .Result.FWOGUSD.FirstOrDefault(c => Convert.ToInt64(c[0]) == lastClosedIndex);

                    foreach (var ohlc in ohlcResponse.Result.FWOGUSD)
                    {
                        long timestamp = Convert.ToInt64(ohlc[0]);
                        decimal open = Convert.ToDecimal(ohlc[1]);
                        decimal high = Convert.ToDecimal(ohlc[2]);
                        decimal low = Convert.ToDecimal(ohlc[3]);
                        decimal close = Convert.ToDecimal(ohlc[4]);
                        decimal vwap = Convert.ToDecimal(ohlc[5]);
                        decimal volume = Convert.ToDecimal(ohlc[6]);
                        int count = Convert.ToInt32(ohlc[7]);

                        if (timestamp > lastClosedIndex)
                        {
                            break;
                        }

                        closePrices.Add(Convert.ToDecimal(ohlc[4], CultureInfo.InvariantCulture)); // Close price is at index 4
                    }

                    var latestRsi = Indicators.CalculateLatestRSI(closePrices.ToList());
                    Console.WriteLine($"Latest RSI: {latestRsi}");

                    var latestRsiWilderSmoothing = Indicators.CalculateLatestRSIWithWilderSmoothing(closePrices);
                    Console.WriteLine($"Latest Wilder Smoothing RSI: {latestRsiWilderSmoothing}");

                    var rsi = Indicators.CalculateRSI(closePrices.ToList());
                    var stohasticRsi = Indicators.CalculateStochRSI(rsi);

                    // Print latest values like TradingView
                    Console.WriteLine($"%K (blue): {stohasticRsi.Item1:F2}");
                    Console.WriteLine($"%D (orange): {stohasticRsi.Item2:F2}");

                    //DowntrendDetector
                    var isDowntrend = DowntrendDetector.IsDowntrend(closePrices);
                    Console.WriteLine($"Is down trend {isDowntrend}");

                    // Buy signal detector
                    var ema9 = Indicators.CalculateEMA(closePrices, 9);
                    var ema21 = Indicators.CalculateEMA(closePrices, 21);
                    var (stochK, stochD) = Indicators.CalculateStochasticRSI(closePrices);

                    var isBuySignal = BuySignalDetector.IsBuySignal(closePrices,
                        ema9?.Select(e => e ?? 0).ToList(),
                        ema21?.Select(e => e ?? 0).ToList(),
                        rsi,
                        stochK,
                        stochD);

                    Console.WriteLine($"Is buy signal: {isBuySignal}");

                    var isSellSignal = SellSignalDetector.IsSellSignal(closePrices,
                        ema9?.Select(e => e ?? 0).ToList(),
                        ema21?.Select(e => e ?? 0).ToList(),
                        rsi,
                        stochK,
                        stochD);

                    Console.WriteLine($"Is sell signal: {isSellSignal}");

                    /*await _timelineAnalysisRepository.AddCurrentAnalysis(CryptoCurrencyType.FWOGUSD.ToString(),
                        latestRsi ?? 0,
                        k: stohasticRsi.Item1 ?? 0,
                        d: stohasticRsi.Item2 ?? 0);*/

                    if (isBuySignal) 
                    {
                        var buyResponse = await GetExecutionPriceAsync(CryptoCurrencyType.FWOGUSD.ToString(), true);

                        await _actionSignalRepository.AddAction(CryptoCurrencyType.FWOGUSD.ToString(),
                            "Buy",
                            buyResponse,
                            latestRsi ?? 0,
                            stohasticRsi.Item1 ?? 0,
                            stohasticRsi.Item2 ?? 0);

                        var latestExecutedAction = await _executedActionRepository.GetLatestAction();

                        if (latestExecutedAction != null && latestExecutedAction.IsSuccessfull
                                && Enum.TryParse(typeof(ExecutedActionType), latestExecutedAction.ExecutedActionType, out var executedActionType)
                                && (ExecutedActionType)executedActionType == ExecutedActionType.Sell)
                        {
                            await _executedActionRepository.AddAction(CryptoCurrencyType.FWOGUSD.ToString(),
                                ExecutedActionType.Buy.ToString(),
                                string.Empty,
                                true,
                                true,
                                buyResponse,
                                latestRsi ?? 0,
                                stohasticRsi.Item1 ?? 0,
                                stohasticRsi.Item2 ?? 0);
                        }
                    }

                    if (isSellSignal)
                    {
                        var sellResponse = await GetExecutionPriceAsync(CryptoCurrencyType.FWOGUSD.ToString(), false);

                        await _actionSignalRepository.AddAction(CryptoCurrencyType.FWOGUSD.ToString(),
                            "Sell",
                            sellResponse,
                            latestRsi ?? 0,
                            stohasticRsi.Item1 ?? 0,
                            stohasticRsi.Item2 ?? 0);

                        var latestExecutedAction = await _executedActionRepository.GetLatestAction();

                        if (latestExecutedAction != null && latestExecutedAction.IsSuccessfull
                                && Enum.TryParse(typeof(ExecutedActionType), latestExecutedAction.ExecutedActionType, out var executedActionType)
                                && (ExecutedActionType)executedActionType == ExecutedActionType.Buy)
                        {
                            await _executedActionRepository.AddAction(CryptoCurrencyType.FWOGUSD.ToString(),
                                ExecutedActionType.Sell.ToString(),
                                string.Empty,
                                true,
                                true,
                                sellResponse,
                                latestRsi ?? 0,
                                stohasticRsi.Item1 ?? 0,
                                stohasticRsi.Item2 ?? 0);
                        }
                    }

                    //await _responseRepository.AddResponse(jsonResponse);
                }
            }
        }

        public async Task<decimal> GetExecutionPriceAsync(string pair, bool isBuy)
        {
            using var client = new HttpClient();
            var url = $"https://api.kraken.com/0/public/Ticker?pair={pair}";
            var response = await client.GetStringAsync(url);

            var json = JsonDocument.Parse(response);
            var result = json.RootElement.GetProperty("result");
            var pairKey = result.EnumerateObject().First().Name;
            var pairData = result.GetProperty(pairKey);

            // Choose "a" (ask) for buy, "b" (bid) for sell
            string priceStr = isBuy
                ? pairData.GetProperty("a")[0].GetString()
                : pairData.GetProperty("b")[0].GetString();

            return decimal.Parse(priceStr, CultureInfo.InvariantCulture);
        }
        public class KrakenResponse
        {
            [JsonProperty("error")]
            public List<string> Error { get; set; }

            [JsonProperty("result")]
            public KrakenResult Result { get; set; }
        }

        public class KrakenResult
        {
            [JsonProperty("FWOGUSD")]
            public List<List<object>> FWOGUSD { get; set; }

            [JsonProperty("last")]
            public long Last { get; set; }
        }
    }
}
