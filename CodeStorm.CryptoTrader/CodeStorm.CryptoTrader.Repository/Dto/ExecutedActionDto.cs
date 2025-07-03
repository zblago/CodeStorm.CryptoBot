namespace CodeStorm.CryptoTrader.Repository.Dto
{
    public class ExecutedActionDto
    {
        public decimal Price { get; set; }
        public string ExecutedActionType { get; set; } // e.g., Buy, Sell, BuyFailed, SellFailed, BackTesting
        public string Message { get; set; }
        public bool IsSuccessfull { get; set; }
    }
}
