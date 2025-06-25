using CodeStorm.CryptoTrader.Repository.DatabaseContext;
using DataFetcher.Repository;
using Microsoft.EntityFrameworkCore;

namespace CodeStorm.CryptoTrader.Repository.Repository
{
    public class TimelineAnalysisRepository
    {
        private readonly CryptoTraderDbContext _cryptoTraderDbContext;

        public TimelineAnalysisRepository(CryptoTraderDbContext cryptoTraderDbContext)
        {
            _cryptoTraderDbContext = cryptoTraderDbContext;
            _cryptoTraderDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public async Task AddCurrentAnalysis(string cryptCurrencyType,
            decimal latestRsi,
            decimal k,
            decimal d)
        {
            _cryptoTraderDbContext.Add(new TimelineAnalysis
            {
                LatestRsi = latestRsi,
                CryptoCurrencyType = cryptCurrencyType,
                K = k,
                D = d,
                CreatedOnUtC = DateTime.UtcNow
            });

            await _cryptoTraderDbContext.SaveChangesAsync();
        }
    }
}
