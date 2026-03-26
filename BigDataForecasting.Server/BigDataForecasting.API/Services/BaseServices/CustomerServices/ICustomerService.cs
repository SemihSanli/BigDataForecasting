using BigDataForecasting.API.Dtos.CustomerDtos;
using BigDataForecasting.API.Dtos.MLDtos;

namespace BigDataForecasting.API.Services.BaseServices.CustomerServices
{
    public interface ICustomerService
    {
        Task<int> GetActiveUsersAsync();
        Task<decimal> GetWalletBalanceAsync();
        //Task<CustomerWithSalesDto> GetCustomerWithSalesByIdDto(int customerId);
        Task<List<CustomerWithSalesDto>> GetAllCustomerWithSalesAsync(int pageNumber, int pageSize);
        Task<List<CustomerWithSalesDto>> GetRandomCustomerAsync(int count);
        Task<List<CLTVInput>> GetCLTVTrainingDataAsync();
        Task<List<GetAllActiveStatusCustomerDto>> GetAllActiveStatusCustomersAsync();
        Task<List<CustomerByCountryCountDto>> GetCustomerCountByCountryAsync();
        Task<List<FullCustomerDetailDto>> GetAllCustomersWithFullDetailsAsync(int pageNumber = 1,
    int pageSize = 10,
    string? searchTerm = null,
    string? sortBy = null);
    }
}
