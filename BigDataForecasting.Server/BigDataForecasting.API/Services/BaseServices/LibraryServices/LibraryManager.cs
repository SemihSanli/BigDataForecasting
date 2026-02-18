using BigDataForecasting.API.Context.ForecastingDb;
using BigDataForecasting.API.Dtos.LibraryDtos;
using BigDataForecasting.API.Entities;
using BigDataForecasting.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BigDataForecasting.API.Services.BaseServices.LibraryServices
{
    public class LibraryManager : ILibraryService
    {
        private readonly IGenericRepository<Sale> _saleRepository;
        private readonly IGenericRepository<Game> _gameRepository;
        private readonly AppDbContext _appDbContext;
        public LibraryManager(IGenericRepository<Sale> saleRepository, IGenericRepository<Game> gameRepository, AppDbContext appDbContext)
        {
            _saleRepository = saleRepository;
            _gameRepository = gameRepository;
            _appDbContext = appDbContext;
        }

        public async Task AddToLibraryAsync(int customerId, int gameId)
        {
           var exists = await _saleRepository.AnyAsync(s=>s.CustomerId==customerId && s.GameId == gameId);
            if (exists)
            {
                throw new Exception("Game is already in the library.");
            }
            var game = await _gameRepository.GetByIdAsync(gameId);
           
            if (game==null)
            {
                throw new Exception("Game not found.");
            }
            var sale = new Sale
            {
                CustomerId = customerId,
                GameId = gameId,
                SaleDate = DateTime.UtcNow,
                SoldPrice = game.Price, 
                PlayTimeHours = 0,
                Rating = null
            };
           await _saleRepository.AddAsync(sale);

            var gameStat = await _appDbContext.GameStats.FirstOrDefaultAsync(gs => gs.GameId == gameId);
            if(gameStat != null)
            {
                gameStat.TotalLibraryAdds += 1;
                gameStat.LastUpdated = DateTime.UtcNow;
            }
            // 5. UserActivity logla (Burası değişecek loglama eklediğmde)
            await _appDbContext.UserActivities.AddAsync(new UserActivity
            {
                CustomerId = customerId,
                ActivityType = "AddToLibrary",
                ActivityDate = DateTime.UtcNow
            });
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<List<LibraryGameDto>> GetUserLibraryAsync(int customerId)
        {
            return await _saleRepository.Where(s => s.CustomerId == customerId)
                  .Select(s => new LibraryGameDto
                  {
                      GameId = s.GameId,
                      GameName = s.Game.GameName,
                      CoverImageUrl = s.Game.CoverImageUrl,
                      Genre = s.Game.Genre,
                      PlayTimeHours = s.PlayTimeHours,
                      Rating = s.Rating,
                      SaleDate = s.SaleDate
                  })
                  .OrderByDescending(s => s.SaleDate)
                  .ToListAsync();
        }

        public async Task RateGameAsync(int customerId, int gameId, double rating)
        {
            // 1. Kütüphanede var mı?
            var sale = await _saleRepository
                .Where(s => s.CustomerId == customerId && s.GameId == gameId, tracking: true)
                .FirstOrDefaultAsync();
            if (sale == null)
                throw new Exception("Bu oyun kütüphanende değil. Önce ekle.");

            // 2. Puanı güncelle
            sale.Rating = rating;

            // 3. O oyunun ortalama rating'ini yeniden hesapla
            var avgRating = await _saleRepository
                .Where(s => s.GameId == gameId && s.Rating != null)
                .Select(s => s.Rating!.Value)
                .DefaultIfEmpty(0)
                .AverageAsync();

            var gameStat = await _appDbContext.GameStats
                .FirstOrDefaultAsync(gs => gs.GameId == gameId);
            if (gameStat != null)
            {
                gameStat.AverageRating = avgRating;
                gameStat.LastUpdated = DateTime.UtcNow;
            }

            // 4. UserActivity logla
            await _appDbContext.UserActivities.AddAsync(new UserActivity
            {
                CustomerId = customerId,
                ActivityType = "Rate",
                ActivityDate = DateTime.UtcNow
            });

            // 5. Tek SaveChanges
            await _appDbContext.SaveChangesAsync();
        }
    }
}
