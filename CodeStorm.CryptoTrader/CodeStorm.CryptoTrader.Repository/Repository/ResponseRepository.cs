using CodeStorm.CryptoTrader.Repository.DatabaseContext;
using DataFetcher.Repository;
using Microsoft.EntityFrameworkCore;

namespace CodeStorm.CryptoTrader.Repository.Repository
{
    public class ResponseRepository
    {
        private readonly CryptoTraderDbContext _cryptoTraderDbContext;

        public ResponseRepository(CryptoTraderDbContext cryptoTraderDbContext)
        {
            _cryptoTraderDbContext = cryptoTraderDbContext;
            _cryptoTraderDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public async Task AddResponse(string response)
        {
            _cryptoTraderDbContext.Add(new Response
            {
                ResponseText = response,
                CreatedOnUtC = DateTime.UtcNow
            });

            await _cryptoTraderDbContext.SaveChangesAsync();
        }

        public async Task AddOhlc(decimal latestRsi,
            string cryptCurrencyType,
            int timestampInteger,
            DateTime timeStampUtc,
            decimal open,
            decimal high,
            decimal low,
            decimal close,
            decimal vwap,
            decimal volume,
            int count)
        {
            _cryptoTraderDbContext.Add(new Ohlc
            {
                LatestRsi = latestRsi,
                CryptoCurrencyType = cryptCurrencyType,
                TimeStampInteger = timestampInteger,
                TimeStampUtc = DateTimeOffset.FromUnixTimeSeconds(timestampInteger).UtcDateTime,
                Open = open,
                High = high,
                Low = low,
                Close = close,
                VWap = vwap,
                Volume = volume,
                Count = count,
                CreatedOnUtC = timeStampUtc
            });

            await _cryptoTraderDbContext.SaveChangesAsync();
        }
    }
}
