namespace BigDataForecasting.API.Dtos.GameDtos
{
    public class GetAllGamesWithDetailsDto
    {
        public int GameId { get; set; }
        public string GameName { get; set; }
        public string Description { get; set; }
        public string Genre { get; set; }
        public decimal Price { get; set; }
        public string CoverImageUrl { get; set; }

        // GameStat'tan gelenler
        public double AverageRating { get; set; }
        public int TotalLibraryAdds { get; set; }

        // GameDetail'den gelenler
        public DateTime? ReleaseDate { get; set; }
        public string? Developer { get; set; }
        public bool IsMultiplayer { get; set; }

        // GameCategory'den gelenler (Çoklu olduğu için liste)
        public List<string> Categories { get; set; } = new List<string>();
    }
}
