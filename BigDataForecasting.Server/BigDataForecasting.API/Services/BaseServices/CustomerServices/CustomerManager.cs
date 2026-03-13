
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
            // EF Core'un LINQ içinde DateTime.Now hesaplarken patlamaması için tarihi dışarı alıyoruz:
            DateTime cutoffDate = DateTime.Now.AddMonths(-6);

            return await _customerRepository.GetAll()
                .Include(c => c.Sales) // <--- BUNU YAZMAZSAK SATIŞLAR GELMEZ, HERKES 0 ÇEKER!
                .AsNoTracking()
                .OrderBy(c => c.CustomerId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CustomerWithSalesDto
                {
                    CustomerId = c.CustomerId,
                    UserName = c.UserName,
                    Input = new CustomerChurnInput
                    {
                        // Sales.Any() kontrolü ekleyerek NullReference hatalarını önlüyoruz
                        TotalMoneySpent = c.Sales.Any() ? (float)c.Sales.Sum(s => s.SoldPrice) : 0f,
                        TotalGamesBought = c.Sales.Any() ? c.Sales.Count() : 0,

                        // Kural: Hiç satışı yoksa (True) VEYA en son satışı 6 aydan eskiyse (True).
                        HasChurned = !c.Sales.Any() || c.Sales.Max(s => s.SaleDate) < cutoffDate,

                        AverageGamePrice = c.Sales.Any() ? (float)c.Sales.Average(s => s.SoldPrice) : 0f
                    }
                })
                .ToListAsync();
        }

        public async Task<List<CLTVInput>> GetCLTVTrainingDataAsync()
        {
            DateTime cutoffDate = DateTime.Now.AddMonths(-6);

            return await _customerRepository.GetAll()
                .Include(c => c.Sales)
                .AsNoTracking()
                // Sadece geçmişte (6 aydan daha önce) alışveriş yapmış köklü müşterilerle eğitim yapıyoruz
                .Where(c => c.Sales.Any(s => s.SaleDate < cutoffDate))
                .Select(c => new CLTVInput
                {
                    // 1. GEÇMİŞ BİLGİLERİ (Yapay zekanın bakacağı özellikler)
                    // 6 ay öncesine kadar ne kadar para harcadı?
                    TotalMoneySpentSoFar = (float)c.Sales.Where(s => s.SaleDate < cutoffDate).Sum(s => s.SoldPrice),

                    // 6 ay öncesine kadar kaç oyun aldı?
                    TotalGamesBoughtSoFar = c.Sales.Count(s => s.SaleDate < cutoffDate),

                    // Mevcut cüzdan bakiyesi
                    WalletBalance = (float)c.WalletBalance,

                    
                    // Kesme tarihinden BUGÜNE kadar (yani son 6 ayda) GERÇEKTE ne kadar harcadı?
                    FutureSpendingTarget = (float)c.Sales.Where(s => s.SaleDate >= cutoffDate).Sum(s => s.SoldPrice)
                })
                .ToListAsync();
        }

        public async Task<List<CustomerWithSalesDto>> GetRandomCustomerAsync(int count)
        {
            return await _customerRepository.GetAll()
         .AsNoTracking()
         .Where(c => c.IsActive == true) // Sadece aktif üyeleri al
         .OrderBy(c => Guid.NewGuid())   // Karıştır!
         .Take(count)                    // İstediğin kadarını al (Örn: 10)
         .Select(c => new CustomerWithSalesDto
         {
             CustomerId = c.CustomerId,
             UserName = c.UserName
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
