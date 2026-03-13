using BigDataForecasting.API.Dtos.CustomerDtos;
using BigDataForecasting.API.Dtos.GameDtos;
using BigDataForecasting.API.Dtos.MLDtos;

namespace BigDataForecasting.API.Services.MLServices
{
    public interface IAITrainerService
    {
        byte[] TrainAndSaveModel(List<CustomerChurnInput> trainingData);
        Task TrainAndSaveModelFromDbAsync();
        //Task<CustomerChurnPrediction> PredictionCustomerChurnAsync(int customerId);
        Task<List<RiskyCustomerResult>> GetTopRiskyCustomerAsync();
        Task<List<float>> PredictionNextMonthsRevenueAsync();
        Task TrainRecommendationModelAsync();
        Task<List<AdminUserRecommendationResultDto>> GetGameRecommendationsForUserAsync(int customerId, int topN = 5);
        Task<List<DashboardRandomCustomerRecommendationDto>> GetRandomCustomerRecommendationsAsync();
        Task TrainCLTVModelAsync();
        Task<List<AdminCLTVResultDto>> GetCLTVPredictionsForAllCustomersAsync();
    }
}
