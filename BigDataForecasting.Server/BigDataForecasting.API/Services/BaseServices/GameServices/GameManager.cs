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
