using BigDataForecasting.API.Dtos.SaleDtos;

namespace BigDataForecasting.API.Services.BaseServices.SaleServices
{
    public interface ISaleService
    {
        Task<List<ResultSaleDto>> GetAllSalesAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task<LastYearSalesReportDto> GetLastYearSalesReportAsync();
        Task<List<MonthlySalesDto>> GetMonthlySalesAsync();
        Task<List<Top5GameSaleDto>> GetTop5BestSellingGamesAsync();
        Task<List<SalesDistributionByGenre>> GetSalesDistributionByGenreAsync();
    }
}
