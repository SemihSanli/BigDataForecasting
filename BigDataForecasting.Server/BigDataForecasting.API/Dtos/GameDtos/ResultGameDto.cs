namespace BigDataForecasting.API.Dtos.GameDtos
{
    public class ResultGameDto
    {
        public string GameName { get; set; }
        public string Description { get; set; }
        public string Genre { get; set; } // ML için önemli (RPG, FPS vs.)
        public decimal Price { get; set; }
        public string CoverImageUrl { get; set; }
        public double AverageRating { get; set; } = 0;
        public int TotalLibraryAdds { get; set; } = 0;
    }
}
