namespace BigDataForecasting.API.Dtos.CustomerDtos
{
    public class CustomerLibraryItemDto
    {
        public int GameId { get; set; }
        public string GameName { get; set; }
        public string? CoverImageUrl { get; set; }
        public double PlayTimeHours { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}
