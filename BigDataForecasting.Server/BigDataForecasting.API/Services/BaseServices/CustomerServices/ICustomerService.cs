using BigDataForecasting.API.Dtos.CustomerDtos;

namespace BigDataForecasting.API.Services.BaseServices.CustomerServices
{
    public interface ICustomerService
    {
        Task<int> GetActiveUsersAsync();
        Task<decimal> GetWalletBalanceAsync();
        //Task<CustomerWithSalesDto> GetCustomerWithSalesByIdDto(int customerId);
        Task<List<CustomerWithSalesDto>> GetAllCustomerWithSalesAsync(int pageNumber, int pageSize);
    }
}
