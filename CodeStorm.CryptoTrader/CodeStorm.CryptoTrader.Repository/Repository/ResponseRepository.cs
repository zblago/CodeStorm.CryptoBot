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
    }
}
