using BigDataForecasting.API.Dtos.WhishlistDtos;

namespace BigDataForecasting.API.Services.BaseServices.WishlistServices
{
    public interface IWishlistService
    {
        Task<List<ResultWishListDto>> GetUserWhishlistAsync(int customerId);
        Task AddToWhishlistAsync(int customerId, int gameId);
        Task RemoveFromWhishlistAsync(int customerId, int gameId);
    }
}
