using BigDataForecasting.API.Dtos.GameDetailDtos;
using BigDataForecasting.API.Dtos.GameDtos;

namespace BigDataForecasting.API.Services.BaseServices.GameServices
{
    public interface IGameService
    {
        Task<List<ResultGameDto>> GetAllGameAsync();
        Task<ResultGamedetailDto?> GetGameDetailAsync(int gameId);
        Task<List<GamesWithCategoryDto>> GetGamesWithCategoryAsync();
        Task<List<GetAllGamesWithBasicDetailsDto>> GetAllGamesWithBasicDetail();
        Task<List<GetAllGamesWithDetailsDto>> GetAllGamesWithFullDetailsAsync(int pageNumber = 1,
     int pageSize = 10,
     string? searchTerm = null,
     string? sortBy = null);
    }
}
