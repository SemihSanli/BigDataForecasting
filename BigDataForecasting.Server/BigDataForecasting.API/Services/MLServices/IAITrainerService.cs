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
    }
}
