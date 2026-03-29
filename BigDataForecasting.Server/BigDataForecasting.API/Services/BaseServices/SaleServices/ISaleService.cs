using BigDataForecasting.API.Dtos.MLDtos;
using BigDataForecasting.API.Dtos.SaleDtos;

namespace BigDataForecasting.API.Services.BaseServices.SaleServices
{
    public interface ISaleService
    {
        Task<List<ResultSaleDto>> GetAllSalesAsync(int pageNumber = 1, int pageSize = 50);
        Task<decimal> GetTotalRevenueAsync();
        Task<LastYearSalesReportDto> GetLastYearSalesReportAsync();
        Task<List<MonthlySalesDto>> GetMonthlySalesAsync();
        Task<List<Top5GameSaleDto>> GetTop5BestSellingGamesAsync();
        Task<List<SalesDistributionByGenre>> GetSalesDistributionByGenreAsync();
        Task<List<GameRecommendationInput>> GetGameRecommendationDataAsync();
        Task<List<GetOwnedGameIdByCustomerDto>> GetOwnedGameIdsByCustomerAsync(int customerId);
        Task<List<GetCustomerOwnedGames>> GetOwnedGameByMultipleCustomerAsync(List<int> customerIds);
    }
}
