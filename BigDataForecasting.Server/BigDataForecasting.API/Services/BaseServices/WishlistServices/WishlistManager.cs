using BigDataForecasting.API.Context.ForecastingDb;
using BigDataForecasting.API.Dtos.WhishlistDtos;
using BigDataForecasting.API.Entities;
using BigDataForecasting.API.Repositories;
using BigDataForecasting.API.Services.Caching;
using Microsoft.EntityFrameworkCore;
using static BigDataForecasting.API.Constants.CacheKeys.CacheKeys;

namespace BigDataForecasting.API.Services.BaseServices.WishlistServices
{
    public class WishlistManager : IWishlistService
    {
        private readonly IGenericRepository<WhishList> _whishlistRepository;
        private readonly AppDbContext _appDbContext;
        private readonly IRedisCachingService _redisCachingService;
        public WishlistManager(IGenericRepository<WhishList> whishlistRepository, AppDbContext appDbContext, IRedisCachingService redisCachingService)
        {
            _whishlistRepository = whishlistRepository;
            _appDbContext = appDbContext;
            _redisCachingService = redisCachingService;
        }

        public async Task AddToWhishlistAsync(int customerId, int gameId)
        {
              //Oyun var mı kontrol et
            var gameExists = await _appDbContext.Games.AnyAsync(g => g.GameId == gameId);
            if(!gameExists)
                throw new Exception("Bu oyun mevcut değil.");

            //Kullanıcının istek listesinde zaten ekli mi kontrol et

            var exists = await _whishlistRepository.AnyAsync(
                 w => w.CustomerId == customerId && w.GameId == gameId);
            if (exists)
                throw new Exception("Bu oyun zaten istek listenizde.");

            //istek listesine ekle
            var whishlistItem = new WhishList
            {
                CustomerId = customerId,
                GameId = gameId,
                AddedDate = DateTime.UtcNow
            };

            await _whishlistRepository.AddAsync(whishlistItem);

            //Logging eklenince düzenlenecek
            await _appDbContext.UserActivities.AddAsync(new UserActivity
            {
                CustomerId = customerId,
                ActivityType = "Wishlist",
                ActivityDate = DateTime.UtcNow
            });

            await _appDbContext.SaveChangesAsync();
            await _redisCachingService.RemoveAsync(WishlistKeys.UserWishlist(customerId));
        }

        public async Task<List<ResultWishListDto>> GetUserWhishlistAsync(int customerId)
        {
            string cacheKey = WishlistKeys.UserWishlist(customerId);
            return await _redisCachingService.GetOrAddAsync(cacheKey, async () =>
            {
                return await _whishlistRepository.Where(w => w.CustomerId == customerId)
                   .Select(w => new ResultWishListDto { /* ... */ })
                   .OrderByDescending(w => w.AddedDate)
                   .ToListAsync();
            }, TimeSpan.FromHours(24)); // Ad
        }

        public async Task RemoveFromWhishlistAsync(int customerId, int gameId)
        {
            //istek listesinden kaldırmadan önce ürün var mı kontrol et sonra kaldır
            var whishlistItem = await _whishlistRepository
                   .Where(w => w.CustomerId == customerId && w.GameId == gameId, tracking: true)
                   .FirstOrDefaultAsync();

            if (whishlistItem == null)
                throw new Exception("Bu oyun istek listenizde bulunamadı.");

            _whishlistRepository.Delete(whishlistItem);
            await _appDbContext.SaveChangesAsync();
            await _redisCachingService.RemoveAsync(WishlistKeys.UserWishlist(customerId));
        }
    }
}
