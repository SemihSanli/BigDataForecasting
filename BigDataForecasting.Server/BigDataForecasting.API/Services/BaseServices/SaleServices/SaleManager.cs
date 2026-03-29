using BigDataForecasting.API.Dtos.MLDtos;
using BigDataForecasting.API.Dtos.SaleDtos;
using BigDataForecasting.API.Entities;
using BigDataForecasting.API.Repositories;
using BigDataForecasting.API.Services.Caching;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using static BigDataForecasting.API.Constants.CacheKeys.CacheKeys;

namespace BigDataForecasting.API.Services.BaseServices.SaleServices
{
    public class SaleManager : ISaleService
    {
        private readonly IGenericRepository<Sale> _saleRepository;
        private readonly IRedisCachingService _redisCachingService;
        public SaleManager(IGenericRepository<Sale> saleRepository, IRedisCachingService redisCachingService)
        {
            _saleRepository = saleRepository;
            _redisCachingService = redisCachingService;
        }


        public async Task<List<ResultSaleDto>> GetAllSalesAsync(int pageNumber = 1, int pageSize = 50)
        {
            return await _saleRepository.GetAll()
         .AsNoTracking() 
         .OrderByDescending(s => s.SaleDate) 
         .Skip((pageNumber - 1) * pageSize)  
         .Take(pageSize)                     
         .Select(s => new ResultSaleDto     
         {
             SaleId = s.SaleId,
             CustomerId = s.CustomerId,
             GameId = s.GameId,
             SaleDate = s.SaleDate,
             SoldPrice = s.SoldPrice,
             PlayTimeHours = s.PlayTimeHours,
             Rating = s.Rating,

             UserName = s.Customer != null ? s.Customer.UserName : null,
             Email = s.Customer != null ? s.Customer.Email : null,
             Gender = s.Customer != null ? s.Customer.Gender : null,
             City = s.Customer != null ? s.Customer.City : null,
             CountryCode = s.Customer != null ? s.Customer.CountryCode : null,
             SteamId = s.Customer != null ? s.Customer.SteamId : null,

             GameName = s.Game != null ? s.Game.GameName : null,
             Description = s.Game != null ? s.Game.Description : null,
             Genre = s.Game != null ? s.Game.Genre : null,
             
           
         })
         .ToListAsync(); 
        }

        public async Task<List<GameRecommendationInput>> GetGameRecommendationDataAsync()
        {
            return await _saleRepository.GetAll()
          .AsNoTracking()
          .Select(s => new GameRecommendationInput
          {
              CustomerId = (uint)s.CustomerId, // ML.NET bu algoritmada uint bekler
              GameId = (uint)s.GameId,
              Label = (float)s.Rating // 0 ile 5 arasındaki gerçek değerlendirme puanı
          })
          .ToListAsync();
        }

        public async Task<LastYearSalesReportDto> GetLastYearSalesReportAsync()
        {
           return await _redisCachingService.GetOrAddAsync(SaleKeys.LastYearReport, async () =>
            {
                var lastYear = DateTime.Now.AddYears(-1);
                var result = await _saleRepository.GetAll()
                    .Where(s => s.SaleDate >= lastYear)
                    .GroupBy(s => 1)
                    .Select(g => new LastYearSalesReportDto
                    {
                        SalesCount = g.Count(),
                        Revenue = g.Sum(s => s.SoldPrice)
                    }).FirstOrDefaultAsync();

                return result ?? new LastYearSalesReportDto { SalesCount = 0, Revenue = 0 };
            }, TimeSpan.FromHours(2)); //
        }

        public async Task<List<MonthlySalesDto>> GetMonthlySalesAsync()
        {
            return await _redisCachingService.GetOrAddAsync(SaleKeys.MonthlySales, async () =>
            {
                var startDate = DateTime.Now.AddYears(-15);
                var rawData = await _saleRepository.GetAll()
                    .Where(s => s.SaleDate >= startDate)
                    .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        MonthNumber = g.Key.Month,
                        TotalRevenue = g.Sum(s => s.SoldPrice)
                    })
                    .OrderBy(x => x.Year).ThenBy(x => x.MonthNumber)
                    .ToListAsync();

                return rawData.Select(x => new MonthlySalesDto
                {
                    Month = new DateTime(x.Year, x.MonthNumber, 1).ToString("MMM yyyy", new CultureInfo("tr-TR")),
                    TotalRevenue = x.TotalRevenue
                }).ToList();
            }, TimeSpan.FromDays(1)); // Geçmiş ayl
        }

        public async Task<List<GetCustomerOwnedGames>> GetOwnedGameByMultipleCustomerAsync(List<int> customerIds)
        {
            return await _saleRepository.GetAll()
          .AsNoTracking()
          .Where(s => customerIds.Contains((int)s.CustomerId))
          .Select(s => new GetCustomerOwnedGames
          {
              CustomerId = (int)s.CustomerId,
              GameId = s.GameId
          })
          .Distinct()
          .ToListAsync();
        }

        public async Task<List<GetOwnedGameIdByCustomerDto>> GetOwnedGameIdsByCustomerAsync(int customerId)
        {
            string cacheKey = SaleKeys.OwnedGames(customerId);

            return await _redisCachingService.GetOrAddAsync(cacheKey, async () =>
            {
                return await _saleRepository.GetAll().AsNoTracking()
                    .Where(s => s.CustomerId == customerId)
                    .Select(s => s.GameId).Distinct()
                    .Select(id => new GetOwnedGameIdByCustomerDto { GameId = id })
                    .ToListAsync();
            }, TimeSpan.FromHours(24));
        }

        public async Task<List<SalesDistributionByGenre>> GetSalesDistributionByGenreAsync()
        {
            return await _redisCachingService.GetOrAddAsync(SaleKeys.DistributionByGenre, async () =>
            {
                return await _saleRepository.GetAll()
                    .GroupBy(s => s.Game.Genre)
                    .Select(g => new SalesDistributionByGenre { Genre = g.Key, SalesCount = g.Count() })
                    .OrderByDescending(x => x.SalesCount)
                    .ToListAsync();
            }, TimeSpan.FromHours(5));
        }

        public async Task<List<Top5GameSaleDto>> GetTop5BestSellingGamesAsync()
        {
            return await _redisCachingService.GetOrAddAsync(SaleKeys.Top5Games, async () =>
            {
                return await _saleRepository.GetAll()
                     .AsNoTracking()
                     .GroupBy(s => new { s.GameId, s.Game.GameName, s.Game.CoverImageUrl })
                     .Select(g => new Top5GameSaleDto
                     {
                         GameId = g.Key.GameId,
                         GameName = g.Key.GameName,
                         CoverImageUrl = g.Key.CoverImageUrl,
                         TotalSalesCount = g.Count(),
                         TotalRevenueGenerated = g.Sum(s => s.SoldPrice)
                     })
                     .OrderByDescending(x => x.TotalSalesCount)
                     .Take(5).ToListAsync();
            }, TimeSpan.FromHours(1)); // Ana sayfadaki en çok satanlar listesi 1 saatte bir yenilensin
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _redisCachingService.GetOrAddAsync(SaleKeys.TotalRevenue, async () =>
            {
                return await _saleRepository.GetAll().SumAsync(s => s.SoldPrice);
            }, TimeSpan.FromHours(1));

        }
    }
}
