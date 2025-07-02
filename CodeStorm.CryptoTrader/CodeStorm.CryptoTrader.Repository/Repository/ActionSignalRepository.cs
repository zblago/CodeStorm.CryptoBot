using CodeStorm.CryptoTrader.Repository.DatabaseContext;
using DataFetcher.Repository;
using Microsoft.EntityFrameworkCore;

namespace CodeStorm.CryptoTrader.Repository.Repository
{
    public class ActionSignalRepository
    {
        private readonly CryptoTraderDbContext _cryptoTraderDbContext;

        public ActionSignalRepository(CryptoTraderDbContext cryptoTraderDbContext)
        {
            _cryptoTraderDbContext = cryptoTraderDbContext;
            _cryptoTraderDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public async Task AddAction(string cryptoCurrencyType,
            string signalType,
            decimal price,
            decimal latestRsi,
            decimal k,
            decimal d)
        {
            _cryptoTraderDbContext.Add(new ActionSignal
            {
                CryptoCurrencyType = cryptoCurrencyType,
                SignalType = signalType,
                Price = price,
                LatestRsi = latestRsi,
                K = k,
                D = d,
                CreatedOnUtC = DateTime.UtcNow
            });

            await _cryptoTraderDbContext.SaveChangesAsync();
        }
    }
}