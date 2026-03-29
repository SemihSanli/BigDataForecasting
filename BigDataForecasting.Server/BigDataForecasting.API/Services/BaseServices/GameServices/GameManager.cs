using BigDataForecasting.API.Dtos.GameDetailDtos;
using BigDataForecasting.API.Dtos.GameDtos;
using BigDataForecasting.API.Entities;
using BigDataForecasting.API.Repositories;
using BigDataForecasting.API.Services.Caching;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using static BigDataForecasting.API.Constants.CacheKeys.CacheKeys;

namespace BigDataForecasting.API.Services.BaseServices.GameServices
{
    public class GameManager : IGameService
    {
        private readonly IGenericRepository<Game> _gameRepository;
        private readonly IRedisCachingService _redisCachingService;
        public GameManager(IGenericRepository<Game> gameRepository, IRedisCachingService redisCachingService)
        {
            _gameRepository = gameRepository;
            _redisCachingService = redisCachingService;
        }

        public async Task<List<ResultGameDto>> GetAllGameAsync()
        {
            return await _redisCachingService.GetOrAddAsync(GameKeys.All, async () =>
            {
                return await _gameRepository.GetAll()
                    .AsNoTracking()
                    .Select(g => new ResultGameDto
                    {
                        GameName = g.GameName,
                        Description = g.Description,
                        Genre = g.Genre,
                        Price = g.Price,
                        CoverImageUrl = g.CoverImageUrl,
                        AverageRating = g.GameStat != null ? g.GameStat.AverageRating : 0,
                        TotalLibraryAdds = g.GameStat != null ? g.GameStat.TotalLibraryAdds : 0
                    })
                    .ToListAsync();
            }, TimeSpan.FromHours(12));
        }

        public async Task<List<GetAllGamesWithBasicDetailsDto>> GetAllGamesWithBasicDetail()
        {
            return await _redisCachingService.GetOrAddAsync(GameKeys.AllBasic, async () =>
            {
                return await _gameRepository.GetAll()
                    .AsNoTracking()
                    .Select(g => new GetAllGamesWithBasicDetailsDto
                    {
                        GameId = g.GameId,
                        GameName = g.GameName,
                        CoverImageUrl = g.CoverImageUrl
                    }).ToListAsync();
            }, TimeSpan.FromDays(1));
        }

        public async Task<List<GetAllGamesWithDetailsDto>> GetAllGamesWithFullDetailsAsync(
   int pageNumber = 1,
     int pageSize = 10,
     string? searchTerm = null,
     string? sortBy = null)
        {
            // KURAL 1: BURASI CACHE'LENMEZ, SQL'E BIRAKILIR.
            var query = _gameRepository.GetAll().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(g =>
                    g.GameName.ToLower().Contains(searchTerm) ||
                    g.Genre.ToLower().Contains(searchTerm) ||
                    (g.GameDetail != null && g.GameDetail.Developer.ToLower().Contains(searchTerm))
                );
            }

            query = sortBy?.ToLower() switch
            {
                "price_desc" => query.OrderByDescending(g => g.Price),
                "price_asc" => query.OrderBy(g => g.Price),
                "name_desc" => query.OrderByDescending(g => g.GameName),
                "name_asc" => query.OrderBy(g => g.GameName),
                "date_desc" => query.OrderByDescending(g => g.GameDetail.ReleaseDate),
                _ => query.OrderByDescending(g => g.GameId)
            };

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(g => new GetAllGamesWithDetailsDto
                {
                    GameId = g.GameId,
                    GameName = g.GameName,
                    Description = g.Description,
                    Genre = g.Genre,
                    Price = g.Price,
                    CoverImageUrl = g.CoverImageUrl,
                    AverageRating = g.GameStat != null ? g.GameStat.AverageRating : 0,
                    TotalLibraryAdds = g.GameStat != null ? g.GameStat.TotalLibraryAdds : 0,
                    ReleaseDate = g.GameDetail != null ? g.GameDetail.ReleaseDate : null,
                    Developer = g.GameDetail != null ? g.GameDetail.Developer : null,
                    IsMultiplayer = g.GameDetail != null && g.GameDetail.IsMultiplayer,
                    Categories = g.GameCategories.Select(gc => gc.GameCategoryName).ToList()
                })
                .ToListAsync(); 
        }

        public async Task<ResultGamedetailDto?> GetGameDetailAsync(int gameId)
        {
            string cacheKey = GameKeys.Detail(gameId);

            return await _redisCachingService.GetOrAddAsync(cacheKey, async () =>
            {
                return await _gameRepository.GetAll()
                    .AsNoTracking()
                    .Where(g => g.GameId == gameId)
                    .Select(g => new ResultGamedetailDto
                    {
                        GameId = g.GameId,
                        GameName = g.GameName,
                        Description = g.Description,
                        Genre = g.Genre,
                        Price = g.Price,
                        CoverImageUrl = g.CoverImageUrl,
                        AverageRating = g.GameStat != null ? g.GameStat.AverageRating : 0,
                        TotalLibraryAdds = g.GameStat != null ? g.GameStat.TotalLibraryAdds : 0,
                        Developer = g.GameDetail != null ? g.GameDetail.Developer : null,
                        ReleaseDate = g.GameDetail != null ? g.GameDetail.ReleaseDate : null,
                        IsMultiplayer = g.GameDetail != null && g.GameDetail.IsMultiplayer,
                        GameCategories = g.GameCategories.Select(gc => gc.GameCategoryName).ToList()
                    }).FirstOrDefaultAsync();
            }, TimeSpan.FromHours(24));
        }

        public async Task<List<GamesWithCategoryDto>> GetGamesWithCategoryAsync()
        {
            return await _redisCachingService.GetOrAddAsync(GameKeys.AllWithCategory, async () =>
            {
                return await _gameRepository.GetAll()
                    .AsNoTracking()
                    .Include(gc => gc.GameCategories)
                    .Select(g => new GamesWithCategoryDto
                    {
                        GameId = g.GameId,
                        GameName = g.GameName,
                        Description = g.Description,
                        Genre = g.Genre,
                        Price = g.Price,
                        CoverImageUrl = g.CoverImageUrl,
                        GameCategoryId = g.GameCategories.FirstOrDefault() != null ? g.GameCategories.FirstOrDefault().GameCategoryId : 0,
                        GameCategoryName = g.GameCategories.FirstOrDefault() != null ? g.GameCategories.FirstOrDefault().GameCategoryName : null
                    }).ToListAsync();
            }, TimeSpan.FromHours(12));
        }
    }
}
