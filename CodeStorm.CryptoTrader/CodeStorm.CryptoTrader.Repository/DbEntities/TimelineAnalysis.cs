using CodeStorm.CryptoTrader.Repository.DbEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataFetcher.Repository
{
    [Table("TimelineAnalysis")]
    public class TimelineAnalysis : IEntity<int>
    {
        public int Id { get; set; }        
        public decimal LatestRsi { get; set; }
        public string CryptoCurrencyType { get; set; }

        public decimal K { get; set; }
        public decimal D { get; set; }

        public DateTime CreatedOnUtC { get; set; }        
    }
}
