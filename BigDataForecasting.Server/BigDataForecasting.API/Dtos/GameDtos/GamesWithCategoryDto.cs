namespace BigDataForecasting.API.Dtos.GameDtos
{
    public class GamesWithCategoryDto
    {
        public int GameId { get; set; }
        public string GameName { get; set; }
        public string Description { get; set; }
        public string Genre { get; set; } 
        public decimal Price { get; set; }
        public string CoverImageUrl { get; set; }

        public int GameCategoryId { get; set; }
        public string GameCategoryName { get; set; }
    }
}
