using BigDataForecasting.API.Extensions;
using BigDataForecasting.API.Services.BaseServices.WishlistServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BigDataForecasting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WishListsController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishListsController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [HttpPost("add/{gameId}")]
        public async Task<IActionResult> AddToWishList(int gameId)
        {
            int userId = User.GetUserId();

            await _wishlistService.AddToWhishlistAsync(userId, gameId);
            return Ok(new { message = "Oyun istek listenize eklendi." });
        }
        [HttpGet("my-wishlist")]
        public async Task<IActionResult> GetMyWishList()
        {
            int userId = User.GetUserId();
            var wishList = await _wishlistService.GetUserWhishlistAsync(userId);
            return Ok(wishList);
        }
        [HttpDelete("remove/{gameId}")]
        public async Task<IActionResult> RemoveFromWishlist(int gameId)
        {
            int userId = User.GetUserId();
            await _wishlistService.RemoveFromWhishlistAsync(userId, gameId);

            return Ok(new { message = "Oyun istek listesinden çıkarıldı." });
        }
    }
}
