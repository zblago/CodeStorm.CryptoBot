namespace CodeStorm.CryptoTrader.Repository.DbEntities
{
    public interface IEntity<T>
    {
        public T Id { get; set; }
    }
}
