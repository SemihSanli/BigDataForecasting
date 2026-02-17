namespace BigDataForecasting.API.Entities
{
    public class PriceHistory
    {
        public int PriceHistoryId { get; set; }

        public int GameId { get; set; }
        public Game Game { get; set; }

        public decimal Price { get; set; }
        public decimal? DiscountPercent { get; set; }
        public DateTime RecordDate { get; set; }
    }
}
