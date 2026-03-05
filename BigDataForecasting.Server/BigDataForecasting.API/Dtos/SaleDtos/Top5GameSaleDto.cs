namespace BigDataForecasting.API.Dtos.SaleDtos
{
    public class Top5GameSaleDto
    {
        public int TotalSalesCount { get; set; }
        public decimal TotalRevenueGenerated { get; set; }
        public int GameId { get; set; }
        public string GameName { get; set; }
        public string CoverImageUrl { get; set; }
    }
}
