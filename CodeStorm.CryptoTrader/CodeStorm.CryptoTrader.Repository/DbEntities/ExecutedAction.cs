using CodeStorm.CryptoTrader.Repository.DbEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataFetcher.Repository
{
    [Table("ExecutedAction")]
    public class ExecutedAction : IEntity<int>
    {
        public int Id { get; set; }
        public string CryptoCurrencyType { get; set; }
        public required string Type { get; set; } // e.g., Buy, Sell, BuyFailed, SellFailed
        public string Message { get; set; }
        public bool IsBackTesting { get; set; }
        public bool IsSuccessfull { get; set; }

        public decimal Price { get; set; }
        public decimal LatestRsi { get; set; }

        public decimal K { get; set; }
        public decimal D { get; set; }

        public DateTime CreatedOnUtC { get; set; }        
    }
}
