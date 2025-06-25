using CodeStorm.CryptoTrader.Application.Utilities;
using CodeStorm.CryptoTrader.Repository.Repository;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Skender.Stock.Indicators;
using System.Globalization;

namespace CodeStorm.CryptoTrader.Application.ApplicationServices
{
    public class RealTimeIndicatorService
    {
        private readonly ResponseRepository _responseRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        public RealTimeIndicatorService(ResponseRepository responseRepository, 
            IHttpClientFactory httpClientFactory) 
        { 
            _responseRepository = responseRepository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task GetLatestOHLC()
        {
            var httpRequestMessage = new HttpRequestMessage
                (HttpMethod.Get, "https://api.kraken.com/0/public/OHLC?pair=FWOGUSD&interval=15");

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

                    List<Quote> quotes = new List<Quote>();

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

                        quotes.Add(new Quote
                        {
                            Date = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime,
                            Open = open,
                            High = high,
                            Low = low,
                            Close = close,
                            Volume = volume
                        });

                        closePrices.Add(Convert.ToDecimal(ohlc[4], CultureInfo.InvariantCulture)); // Close price is at index 4
                    }

                    var latestRsi = Indicators.CalculateLatestRSIWithWilderSmoothing(closePrices);
                    Console.WriteLine($"Latest RSI: {latestRsi}");

                    var rsi = Indicators.CalculateRSI(closePrices.ToList());
                    var stohasticRsi = Indicators.CalculateStochRSI(rsi);

                    var stochRsiResults = StochRsiCalculator.CalculateStochRsi(closePrices.TakeLast(300).ToList());
                    var latest = stochRsiResults.LastOrDefault(r => r.PercentK.HasValue && r.PercentD.HasValue);
                    Console.WriteLine($"Latest %K = {latest.PercentK:F2}, %D = {latest.PercentD:F2}");


                    var stochRsiResult2 = StochRsiCalculator2.CalculateStochasticRsi(closePrices, 14, 14, 3, 3);

                    var stochRsiResult2Latest = stochRsiResult2.LastOrDefault(x => x.PercentD.HasValue);

                    Console.WriteLine($"Latest RSI: {stochRsiResult2Latest?.Rsi:F2}");
                    Console.WriteLine($"Latest %K: {stochRsiResult2Latest?.PercentK:F2}");
                    Console.WriteLine($"Latest %D: {stochRsiResult2Latest?.PercentD:F2}");
                    

                    var results = quotes.GetStochRsi(14, 14, 3, 3);
                    var latestStochRsi = results.LastOrDefault();

                    Console.WriteLine($"%K: {latestStochRsi?.StochRsi.Value/100:P}");
                    Console.WriteLine($"%D: {latestStochRsi?.Signal/100:P}");


                    //var latest = stohasticRsi.LastOrDefault();
                    //Console.WriteLine($"StochRSI: {latest?.StochRsi:P}");
                    //Console.WriteLine($"%K: {latest?.PercentK:P}");
                    //Console.WriteLine($"%D: {latest?.PercentD:P}");
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
