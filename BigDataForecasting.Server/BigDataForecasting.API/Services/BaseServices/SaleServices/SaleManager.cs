using BigDataForecasting.API.Dtos.SaleDtos;
using BigDataForecasting.API.Entities;
using BigDataForecasting.API.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BigDataForecasting.API.Services.BaseServices.SaleServices
{
    public class SaleManager : ISaleService
    {
        private readonly IGenericRepository<Sale> _saleRepository;

        public SaleManager(IGenericRepository<Sale> saleRepository)
        {
            _saleRepository = saleRepository;
        }

     
        public async Task<List<ResultSaleDto>> GetAllSalesAsync()
        {
            return await _saleRepository.GetAll()
             
                .Include(s => s.Customer)
              
                .Include(s => s.Game)
               
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
                    Genre = s.Game != null ? s.Game.Genre : null
                })
                .ToListAsync();
        }

        public async Task<LastYearSalesReportDto> GetLastYearSalesReportAsync()
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
        }

        public async Task<List<MonthlySalesDto>> GetMonthlySalesAsync()
        {
            var startDate = DateTime.Now.AddYears(-15);

           // Yıl ve Ay'a göre grupluyoruz
            var rawData = await _saleRepository.GetAll()
                .Where(s => s.SaleDate >= startDate)
                .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month }) // Hem Yıl hem Ay
                .Select(g => new
                {
                    Year = g.Key.Year,
                    MonthNumber = g.Key.Month,
                    TotalRevenue = g.Sum(s => s.SoldPrice)
                })
                .OrderBy(x => x.Year)      
                .ThenBy(x => x.MonthNumber) 
                .ToListAsync();

           
            var result = rawData.Select(x => new MonthlySalesDto
            {
               
                Month = new DateTime(x.Year, x.MonthNumber, 1)
                          .ToString("MMM yyyy", new CultureInfo("tr-TR")),
                TotalRevenue = x.TotalRevenue
            }).ToList();
            return result;
        }

        public async Task<List<SalesDistributionByGenre>> GetSalesDistributionByGenreAsync()
        {
           return await _saleRepository.GetAll()
                .GroupBy(s=> s.Game.Genre)
                .Select(g=> new SalesDistributionByGenre
                {
                    Genre = g.Key,
                    SalesCount = g.Count()
                })
                .OrderByDescending(x=>x.SalesCount)
                .ToListAsync();
        }

        public async Task<List<Top5GameSaleDto>> GetTop5BestSellingGamesAsync()
        {
            return await _saleRepository.GetAll()
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
                 .Take(5)
                 .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _saleRepository.GetAll().SumAsync(s => s.SoldPrice);
            
        }
    }
}
