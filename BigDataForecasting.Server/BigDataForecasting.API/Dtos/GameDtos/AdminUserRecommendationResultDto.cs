namespace BigDataForecasting.API.Dtos.GameDtos
{
    public class AdminUserRecommendationResultDto
    {
        public int GameId { get; set; }
        public string GameName { get; set; }
        public float RecommendationScore { get; set; }

        public string? CoverImageUrl { get; set; }
    }
}
