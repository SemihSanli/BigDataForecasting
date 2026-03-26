using BigDataForecasting.API.Dtos.GameDetailDtos;
using BigDataForecasting.API.Dtos.GameDtos;
using BigDataForecasting.API.Entities;
using BigDataForecasting.API.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace BigDataForecasting.API.Services.BaseServices.GameServices
{
    public class GameManager : IGameService
    {
        private readonly IGenericRepository<Game> _gameRepository;
        public GameManager(IGenericRepository<Game> gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<List<ResultGameDto>> GetAllGameAsync()
        {
            return await _gameRepository.GetAll()
                .Select(g=> new ResultGameDto
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
        }

        public async Task<List<GetAllGamesWithBasicDetailsDto>> GetAllGamesWithBasicDetail()
        {
           return await _gameRepository.GetAll()
                .Select(g => new GetAllGamesWithBasicDetailsDto
                {
                    GameId = g.GameId,
                    GameName = g.GameName
                }).ToListAsync();
        }

        public async Task<List<GetAllGamesWithDetailsDto>> GetAllGamesWithFullDetailsAsync(
     int pageNumber = 1,
     int pageSize = 10,
     string? searchTerm = null,
     string? sortBy = null)
        {
            // 1. IQueryable başlatıyoruz
            var query = _gameRepository.GetAll();

            // 2. FİLTRELEME
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(g =>
                    g.GameName.ToLower().Contains(searchTerm) ||
                    g.Genre.ToLower().Contains(searchTerm) ||
                    (g.GameDetail != null && g.GameDetail.Developer.ToLower().Contains(searchTerm))
                );
            }

            // 3. SIRALAMA
            query = sortBy?.ToLower() switch
            {
                "price_desc" => query.OrderByDescending(g => g.Price),
                "price_asc" => query.OrderBy(g => g.Price),
                "name_desc" => query.OrderByDescending(g => g.GameName),
                "name_asc" => query.OrderBy(g => g.GameName),
                "date_desc" => query.OrderByDescending(g => g.GameDetail.ReleaseDate),
                _ => query.OrderByDescending(g => g.GameId)
            };

            // 4. SAYFALAMA VE MAPLEME (DÜZ LİSTE DÖNER)
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
                .ToListAsync(); // Veritabanına gidip sadece o sayfanın verisini List olarak çeker.
        }

        public async Task<ResultGamedetailDto?> GetGameDetailAsync(int gameId)
        {
            return await _gameRepository.Where(g => g.GameId == gameId)
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
        }

        public async Task<List<GamesWithCategoryDto>> GetGamesWithCategoryAsync()
        {
           return await _gameRepository.GetAll()
                .Include(gc=>gc.GameCategories)
                .Select(g=> new GamesWithCategoryDto
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
        }
    }
}
