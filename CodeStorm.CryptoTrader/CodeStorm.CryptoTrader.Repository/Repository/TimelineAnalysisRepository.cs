using CodeStorm.CryptoTrader.Repository.DatabaseContext;
using DataFetcher.Repository;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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
            decimal d,
            decimal currentRSI,
            decimal prevRSI,
            decimal currentEma9,
            decimal currentEma21)
        {
            _cryptoTraderDbContext.Add(new TimelineAnalysis
            {
                LatestRsi = latestRsi,
                CryptoCurrencyType = cryptCurrencyType,
                K = k,
                D = d,
                CurrentRSI = currentRSI,
                PrevRSI = prevRSI,
                CurrentEma9 = currentEma9,
                CurrentEma21 = currentEma21,
                CreatedOnUtC = DateTime.UtcNow
            });

            await _cryptoTraderDbContext.SaveChangesAsync();
        }
    }
}
