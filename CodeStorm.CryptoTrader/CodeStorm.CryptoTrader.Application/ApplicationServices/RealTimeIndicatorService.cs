using CodeStorm.CryptoTrader.Application.Utilities;
using CodeStorm.CryptoTrader.Repository.Repository;
using Newtonsoft.Json;
using System.Globalization;

namespace CodeStorm.CryptoTrader.Application.ApplicationServices
{
    public class RealTimeIndicatorService
    {
        private readonly ResponseRepository _responseRepository;
        private readonly TimelineAnalysisRepository _timelineAnalysisRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        public RealTimeIndicatorService(ResponseRepository responseRepository,
            TimelineAnalysisRepository timelineAnalysisRepository,
            IHttpClientFactory httpClientFactory) 
        { 
            _responseRepository = responseRepository;
            _httpClientFactory = httpClientFactory;
            _timelineAnalysisRepository = timelineAnalysisRepository;
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

                    var latestRsi = Indicators.CalculateLatestRSIWithWilderSmoothing(closePrices);
                    Console.WriteLine($"Latest RSI: {latestRsi}");

                    var rsi = Indicators.CalculateRSI(closePrices.ToList());
                    var stohasticRsi = Indicators.CalculateStochRSI(rsi);

                    // Print latest values like TradingView
                    Console.WriteLine($"%K (blue): {stohasticRsi.Item1:F2}");
                    Console.WriteLine($"%D (orange): {stohasticRsi.Item2:F2}");

                    await _timelineAnalysisRepository.AddCurrentAnalysis(CryptoCurrencyType.FWOGUSD.ToString(),
                        latestRsi ?? 0,
                        k: stohasticRsi.Item1 ?? 0,
                        d: stohasticRsi.Item2 ?? 0);

                    //await _responseRepository.AddResponse(jsonResponse);
                }
            }
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
