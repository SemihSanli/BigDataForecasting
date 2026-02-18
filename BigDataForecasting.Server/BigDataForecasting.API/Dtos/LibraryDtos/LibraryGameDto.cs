namespace BigDataForecasting.API.Dtos.LibraryDtos
{
    public class LibraryGameDto
    {
        public int GameId { get; set; }
        public string GameName { get; set; }
        public string CoverImageUrl { get; set; }
        public string Genre { get; set; }
        public double PlayTimeHours { get; set; }
        public double? Rating { get; set; }
        public decimal SoldPrice { get; set; }
        public DateTime SaleDate { get; set; }
    }
}
