using CodeStorm.CryptoTrader.Repository.DatabaseContext;
using CodeStorm.CryptoTrader.Repository.Dto;
using DataFetcher.Repository;
using Microsoft.EntityFrameworkCore;

namespace CodeStorm.CryptoTrader.Repository.Repository
{
    public class ExecutedActionRepository
    {
        private readonly CryptoTraderDbContext _cryptoTraderDbContext;

        public ExecutedActionRepository(CryptoTraderDbContext cryptoTraderDbContext)
        {
            _cryptoTraderDbContext = cryptoTraderDbContext;
            _cryptoTraderDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public async Task AddAction(string cryptoCurrencyType,
            string type,
            string message,
            bool isBackTesting,
            bool isSuccessfull,
            decimal price,
            decimal latestRsi,
            decimal k,
            decimal d)
        {
            _cryptoTraderDbContext.Add(new ExecutedAction
            {
                CryptoCurrencyType = cryptoCurrencyType,
                Type = type,
                Message = message,
                IsBackTesting = isBackTesting,
                IsSuccessfull = isSuccessfull,
                Price = price,
                LatestRsi = latestRsi,
                K = k,
                D = d,
                CreatedOnUtC = DateTime.UtcNow
            });

            await _cryptoTraderDbContext.SaveChangesAsync();
        }

        public async Task<ExecutedActionDto?> GetLatestAction()
        {
            var latestExecutedAction = await _cryptoTraderDbContext
                .ExecutedActions
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            return latestExecutedAction == null 
                ? null
                : new ExecutedActionDto
            {
                Price = latestExecutedAction.Price,
                ExecutedActionType = latestExecutedAction.Type,
                Message = latestExecutedAction.Message,
                IsSuccessfull = latestExecutedAction.IsSuccessfull
            };
        }
    }
}