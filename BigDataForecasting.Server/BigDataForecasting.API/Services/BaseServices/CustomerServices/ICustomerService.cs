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
    }
}
