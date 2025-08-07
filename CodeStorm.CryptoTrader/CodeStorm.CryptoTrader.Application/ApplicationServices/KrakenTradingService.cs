using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CodeStorm.CryptoTrader.Application.ApplicationServices
{
    public class KrakenTradingService
    {
        private const string ApiKey = "k2s0u2/EkSzOK9Ep32q0i8XYix4AEJXqBw7ZSytFQq891UsXoCFQc1As";
        private const string ApiSecret = "Bk2xCldwV2ubTdHCrRS7aY/784OrzRZnvw6fIuAdRuo/htGnAVPRuZNJXMtlHCybM/I9EgnYYuK/JaS2z/hIFQ==";

        private const string BaseUrl = "https://api.kraken.com";
        private static readonly HttpClient http = new HttpClient();

        public async Task ExecuteLimitBuyAsync(string pair, decimal limitPrice)
        {
            var balance = await GetBalanceAsync();
            if (!balance.Result.TryGetValue("ZUSD", out decimal usd))
            {
                Console.WriteLine("No USD balance available.");
                return;
            }

            decimal volume = Math.Floor((usd / limitPrice) * 1_000_000_000m) / 1_000_000_000m;

            Console.WriteLine($"Placing BUY limit order at {limitPrice} for volume {volume}");

            var result = await AddOrderAsync(pair, "buy", "limit", volume, limitPrice);
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        public async Task ExecuteLimitSellAsync(string pair, decimal limitPrice)
        {
            var asset = GetBaseAsset(pair);
            var balance = await GetBalanceAsync();

            if (!balance.Result.TryGetValue(asset, out decimal cryptoAmount))
            {
                Console.WriteLine($"No {asset} balance available.");
                return;
            }

            decimal volume = Math.Floor(cryptoAmount * 1_000_000_000m) / 1_000_000_000m;

            Console.WriteLine($"Placing SELL limit order at {limitPrice} for volume {volume}");

            var result = await AddOrderAsync(pair, "sell", "limit", volume, limitPrice);
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        public async Task CancelOpenOrdersAsync(string pair)
        {
            var openOrders = await GetOpenOrdersAsync();
            var orderIdsToCancel = new List<string>();

            foreach (var order in openOrders.Result.Open)
            {
                if (order.Value.Descr.Pair == pair)
                    orderIdsToCancel.Add(order.Key);
            }

            foreach (var orderId in orderIdsToCancel)
            {
                Console.WriteLine($"Cancelling order: {orderId}");
                await CancelOrderAsync(orderId);
            }

            if (!orderIdsToCancel.Any())
            {
                Console.WriteLine($"No open orders found for {pair}.");
            }
        }

        private string GetBaseAsset(string pair)
        {
            return pair.Replace("USD", "").Replace("Z", "").Replace("X", "");
        }

        private async Task<Dictionary<string, dynamic>> GetTickerAsync(string pair)
        {
            var response = await http.GetStringAsync($"{BaseUrl}/0/public/Ticker?pair={pair}");
            var json = JsonConvert.DeserializeObject<dynamic>(response);
            return json.result.First.First.Value.ToObject<Dictionary<string, dynamic>>();
        }

        private async Task<KrakenBalanceResponse> GetBalanceAsync()
        {
            var response = await PrivateKrakenRequestAsync("/0/private/Balance", "");

            KrakenBalanceResponse responseObj = JsonConvert.DeserializeObject<KrakenBalanceResponse>(response);

            return responseObj;
        }

        private async Task<dynamic> AddOrderAsync(string pair, string type, string orderType, decimal volume, decimal price)
        {
            string parameters = $"pair={pair}&type={type}&ordertype={orderType}&price={price.ToString().Replace(',', '.')}&volume={volume.ToString().Replace(',', '.')}";
            return await PrivateKrakenRequestAsync("/0/private/AddOrder", parameters);
        }

        private async Task<KrakenResponse> GetOpenOrdersAsync()
        {
            var response = await PrivateKrakenRequestAsync("/0/private/OpenOrders", "");

            return JsonConvert.DeserializeObject<KrakenResponse>(response.ToString());
        }

        private async Task<dynamic> CancelOrderAsync(string txid)
        {
            string parameters = $"txid={Uri.EscapeDataString(txid)}";
            return await PrivateKrakenRequestAsync("/0/private/CancelOrder", parameters);
        }

        private async Task<dynamic> PrivateKrakenRequestAsync(string path, string postData)
        {
            string nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            string data = $"nonce={nonce}&{postData}";                        

            // Step 4: Prepare the HTTP request
            var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + path);

            var apiSign = GetSignature(ApiSecret, nonce, path, data);
            request.Headers.Add("User-Agent", "CryptoTrader");
            request.Headers.Add("API-Key", ApiKey);
            request.Headers.Add("API-Sign", apiSign);
            request.Content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");

            // Step 5: Send request
            var response = await http.SendAsync(request);
            string responseText = await response.Content.ReadAsStringAsync();

            return responseText;
        }

        private async Task<dynamic> PrivateKrakenRequestPostJsonAsync(string path, string postData)
        {
            string nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            string data = $"nonce={nonce}&{postData}";

            // Step 4: Prepare the HTTP request
            var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + path);

            var apiSign = GetSignature(ApiSecret, nonce, path, data);
            request.Headers.Add("User-Agent", "CryptoTrader");
            request.Headers.Add("API-Key", ApiKey);
            request.Headers.Add("API-Sign", apiSign);
            request.Content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");

            // Step 5: Send request
            var response = await http.SendAsync(request);
            string responseText = await response.Content.ReadAsStringAsync();

            return responseText;
        }

        private string GetSignature(string privateKey, string nonce, string path, string bodyString)
        {
            // Combine query string and body (like JS code)
            string postData = bodyString ?? "";

            // SHA256(nonce + postData)
            byte[] sha256Hash;
            using (var sha256 = SHA256.Create())
            {
                sha256Hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(nonce + postData));
            }

            // Concatenate path + SHA256 hash
            byte[] pathBytes = Encoding.UTF8.GetBytes(path);
            byte[] message = new byte[pathBytes.Length + sha256Hash.Length];
            Buffer.BlockCopy(pathBytes, 0, message, 0, pathBytes.Length);
            Buffer.BlockCopy(sha256Hash, 0, message, pathBytes.Length, sha256Hash.Length);

            // HMAC-SHA512
            byte[] privateKeyDecoded = Convert.FromBase64String(privateKey);
            using var hmac = new HMACSHA512(privateKeyDecoded);
            byte[] signatureBytes = hmac.ComputeHash(message);

            return Convert.ToBase64String(signatureBytes);
        }
    }

    public class KrakenResponse
    {
        public List<string> Error { get; set; }
        public KrakenResult Result { get; set; }
    }

    public class KrakenResult
    {
        public Dictionary<string, KrakenOrder> Open { get; set; }
    }

    public class KrakenOrder
    {
        public string Refid { get; set; }
        public string Userref { get; set; }
        public string Status { get; set; }

        public KrakenOrderDescription Descr { get; set; }
    }

    public class KrakenOrderDescription
    { 
        public string Pair { get; set; }
    }

    public class KrakenBalanceResponse
    {
        [JsonPropertyName("error")]
        public List<string> Error { get; set; }

        [JsonPropertyName("result")]
        public Dictionary<string, decimal> Result { get; set; }
    }

}
