
using BigDataForecasting.API.Dtos.CustomerDtos;
using BigDataForecasting.API.Dtos.MLDtos;
using BigDataForecasting.API.Entities;
using BigDataForecasting.API.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

namespace BigDataForecasting.API.Services.BaseServices.CustomerServices
{
    public class CustomerManager : ICustomerService
    {
        private readonly IGenericRepository<Customer> _customerRepository;

        public CustomerManager(IGenericRepository<Customer> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<int> GetActiveUsersAsync()
        {
            return await _customerRepository.GetAll()
                 .Where(c => c.IsActive == true)
                 .CountAsync();
        }

        public async Task<List<CustomerWithSalesDto>> GetAllCustomerWithSalesAsync(int pageNumber, int pageSize)
        {
            return await _customerRepository.GetAll()
         .AsNoTracking() // Veriyi sadece okuyoruz, takip etmeye gerek yok (Hız kazandırır)
         .Skip((pageNumber - 1) * pageSize)
         .Take(pageSize)
        .Select(c => new CustomerWithSalesDto
        {
            CustomerId = c.CustomerId,
            UserName = c.UserName,
            Input = new CustomerChurnInput
            {
                TotalMoneySpent = (float)c.Sales.Sum(s => s.SoldPrice),
                TotalGamesBought = c.Sales.Count(),
                DaysSinceLastLogin = c.LastLoginDate.HasValue
            ? EF.Functions.DateDiffDay(c.LastLoginDate.Value, DateTime.Now)
            : 999,

                // BURASI ÇOK ÖNEMLİ: Yeni tabloları bağla
            }
        })
         .ToListAsync();
        }

        //public async Task<CustomerWithSalesDto> GetCustomerWithSalesByIdDto(int customerId)
        //{
        //    return await _customerRepository
        //   .Where(c => c.CustomerId == customerId)
        //   .Select(c => new CustomerWithSalesDto
        //   {
        //       Id = c.CustomerId,
        //       UserName = c.UserName,
        //       SteamId = c.SteamId,
        //       LastLoginDate = c.LastLoginDate,
        //       Sales = c.Sales.Select(s => new CustomerSaleDetailDto
        //       {
        //           SaleId = s.CustomerId,
        //           SoldPrice = s.SoldPrice,
        //           GameId = s.GameId,
        //           SaleDate = s.SaleDate
        //       }).ToList()
        //   }).FirstOrDefaultAsync();
        //}

        public async Task<decimal> GetWalletBalanceAsync()
        {
            return await _customerRepository.GetAll()
                .Where(c => c.IsActive == true)
                .SumAsync(c => c.WalletBalance);
        }
    }
}
