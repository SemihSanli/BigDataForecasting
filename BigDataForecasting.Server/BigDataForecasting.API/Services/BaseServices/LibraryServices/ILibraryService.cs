using BigDataForecasting.API.Dtos.LibraryDtos;

namespace BigDataForecasting.API.Services.BaseServices.LibraryServices
{
    public interface ILibraryService
    {
        Task<List<LibraryGameDto>> GetUserLibraryAsync(int customerId);
        Task AddToLibraryAsync(int customerId, int gameId);
        Task RateGameAsync(int customerId, int gameId, double rating);
    }
}
