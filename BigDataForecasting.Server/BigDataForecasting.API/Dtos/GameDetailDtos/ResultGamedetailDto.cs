namespace BigDataForecasting.API.Dtos.GameDetailDtos
{
    public class ResultGamedetailDto
    {
        public int GameId { get; set; }
        public string GameName { get; set; }
        public string Description { get; set; }
        public string Genre { get; set; }
        public decimal Price { get; set; }
        public string CoverImageUrl { get; set; }
        public double AverageRating { get; set; }
        public int TotalLibraryAdds { get; set; }
        public string? Developer { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public bool IsMultiplayer { get; set; }
        public List<string> GameCategories { get; set; }
    }
}
