using CodeStorm.CryptoTrader.Repository.DbEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataFetcher.Repository
{
    [Table("Ohlc")]
    public class Ohlc : IEntity<int>
    {
        public int Id { get; set; }
        
        public decimal LatestRsi { get; set; }

        public string CryptoCurrencyType { get; set; }
        public int TimeStampInteger { get; set; }
        public DateTime TimeStampUtc { get; set; }

        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }

        /// <summary>
        /// Volume Weighted Average Price
        /// </summary>
        public decimal VWap { get; set; }
        public decimal Volume { get; set; }
        public int Count { get; set; }

        public DateTime CreatedOnUtC { get; set; }        
    }
}
