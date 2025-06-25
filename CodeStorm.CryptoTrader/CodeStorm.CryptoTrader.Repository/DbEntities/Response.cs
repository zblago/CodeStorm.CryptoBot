using CodeStorm.CryptoTrader.Repository.DbEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataFetcher.Repository
{
    [Table("Response")]
    public class Response : IEntity<int>
    {
        public int Id { get; set; }
        public required string ResponseText { get; set; }
        public DateTime CreatedOnUtC { get; set; }
    }
}
