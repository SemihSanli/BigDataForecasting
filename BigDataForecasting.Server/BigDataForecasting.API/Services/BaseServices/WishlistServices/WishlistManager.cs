using BigDataForecasting.API.Context.ForecastingDb;
using BigDataForecasting.API.Dtos.WhishlistDtos;
using BigDataForecasting.API.Entities;
using BigDataForecasting.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BigDataForecasting.API.Services.BaseServices.WishlistServices
{
    public class WishlistManager : IWishlistService
    {
        private readonly IGenericRepository<WhishList> _whishlistRepository;
        private readonly AppDbContext _appDbContext;
        public WishlistManager(IGenericRepository<WhishList> whishlistRepository, AppDbContext appDbContext)
        {
            _whishlistRepository = whishlistRepository;
            _appDbContext = appDbContext;
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
        }

        public async Task<List<ResultWishListDto>> GetUserWhishlistAsync(int customerId)
        {
            //Kullanıcıya göre istek listesini getir en son ekleme tarihine göre
            return await _whishlistRepository.Where(w => w.CustomerId == customerId)
               .Select(w => new ResultWishListDto
               {
                   WishlistId = w.WishlistId,
                   GameId = w.GameId,
                   GameName = w.Game.GameName,
                   CoverImageUrl = w.Game.CoverImageUrl,
                   Genre = w.Game.Genre,
                   Price = w.Game.Price,
                   AddedDate = w.AddedDate
               })
               .OrderByDescending(w => w.AddedDate)
               .ToListAsync();
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
        }
    }
}
