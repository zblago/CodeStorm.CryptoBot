using CodeStorm.CryptoTrader.Repository.DbEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataFetcher.Repository
{
    [Table("ActionSignal")]
    public class ActionSignal : IEntity<int>
    {
        public int Id { get; set; }
        public string CryptoCurrencyType { get; set; }

        public required string SignalType { get; set; } // e.g., Buy, Sell, Hold

        public decimal Price { get; set; }
        public decimal LatestRsi { get; set; }

        public decimal K { get; set; }
        public decimal D { get; set; }

        public DateTime CreatedOnUtC { get; set; }        
    }
}
