using BigDataForecasting.API.Dtos.GameDtos;

namespace BigDataForecasting.API.Dtos.CustomerDtos
{
    public class DashboardRandomCustomerRecommendationDto
    {
        public int CustomerId { get; set; }
        public string UserName { get; set; }
        public List<AdminUserRecommendationResultDto> RecommendedGames { get; set; }
    }
}
