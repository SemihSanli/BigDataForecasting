
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

        public async Task<List<GetAllActiveStatusCustomerDto>> GetAllActiveStatusCustomersAsync()
        {
            return await _customerRepository.GetAll()
                 .AsNoTracking()
                .Where(s => s.IsActive == true)
               .Select(c => new GetAllActiveStatusCustomerDto
               {
                   CustomerId = c.CustomerId,
                   UserName = c.UserName,
                   ProfileImageUrl = c.ProfileImageUrl,
                   TotalSpent = (float)c.Sales.Sum(s => s.SoldPrice),
                   TotalGames = c.Sales.Count(),
                   WalletBalance = c.WalletBalance
               })
               .ToListAsync();

        }

        public async Task<List<FullCustomerDetailDto>> GetAllCustomersWithFullDetailsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string? sortBy = null)
        {
            var query = _customerRepository.GetAll().AsNoTracking().Where(c => c.IsActive);

            // 2. FİLTRELEME (Arama çubuğu)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(c =>
                    c.UserName.ToLower().Contains(searchTerm) ||
                    c.Email.ToLower().Contains(searchTerm) ||
                    (c.FirstName != null && c.FirstName.ToLower().Contains(searchTerm)) ||
                    (c.LastName != null && c.LastName.ToLower().Contains(searchTerm))
                );
            }

            // 3. SIRALAMA
            query = sortBy?.ToLower() switch
            {
                "balance_desc" => query.OrderByDescending(c => c.WalletBalance),
                "date_desc" => query.OrderByDescending(c => c.CreatedDate),
                "playtime_desc" => query.OrderByDescending(c => c.Sales.Sum(s => s.PlayTimeHours)), // En çok oynayan "No-Life" tayfa
                "gamecount_desc" => query.OrderByDescending(c => c.Sales.Count()), // Kütüphanesi en kabarık "Balinalar"
                _ => query.OrderByDescending(c => c.CustomerId) // Default
            };

            // 4. SAYFALAMA VE TEK SQL İLE PROJECTION (Include YOK!)
            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new FullCustomerDetailDto
                {
                    CustomerId = c.CustomerId,
                    UserName = c.UserName,
                    Email = c.Email,
                    FullName = c.FirstName + " " + c.LastName,
                    ProfileImageUrl = c.ProfileImageUrl,
                    CountryCode = c.CountryCode,
                    WalletBalance = c.WalletBalance,
                    CreatedDate = c.CreatedDate,

                    // Alt tablolardan istatistikleri SQL'de hesaplatıyoruz
                    OwnedGameCount = c.Sales.Count(),
                    WishlistGameCount = c.WhishLists.Count(), // Senin entity'deki isme (WhishLists) göre
                    TotalPlayTimeHours = c.Sales.Any() ? c.Sales.Sum(s => s.PlayTimeHours) : 0,

                    // Kullanıcının kütüphanesi (Satın aldığı oyunlar)
                    Library = c.Sales.Select(s => new CustomerLibraryItemDto
                    {
                        GameId = s.GameId,
                        GameName = s.Game.GameName,
                        CoverImageUrl = s.Game.CoverImageUrl,
                        PlayTimeHours = s.PlayTimeHours,
                        PurchasePrice = s.SoldPrice,
                        PurchaseDate = s.SaleDate
                    }).ToList(),

                    // Kullanıcının istek listesi
                    Wishlist = c.WhishLists.Select(w => new CustomerWishListItemDto
                    {
                        GameId = w.GameId,
                        GameName = w.Game.GameName,
                        AddedDate = w.AddedDate
                    }).ToList()

                })
                .AsSplitQuery()
                .ToListAsync();
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

        public async Task<List<CustomerByCountryCountDto>> GetCustomerCountByCountryAsync()
        {
            return await _customerRepository.GetAll()
                .AsNoTracking()
                .Where(c => c.IsActive == true) // Sadece aktif üyeleri al
                .GroupBy(c => c.CountryCode)
                .Select(g => new CustomerByCountryCountDto
                {
                    CountryCode = g.Key,
                    CustomerCount = g.Count()
                })
                .OrderByDescending(x=>x.CustomerCount)
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
             UserName = c.UserName,
             ProfileImageUrl = c.ProfileImageUrl
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
