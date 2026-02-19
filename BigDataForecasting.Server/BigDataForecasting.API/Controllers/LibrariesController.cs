using BigDataForecasting.API.Dtos.LibraryDtos;
using BigDataForecasting.API.Extensions;
using BigDataForecasting.API.Services.BaseServices.LibraryServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BigDataForecasting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LibrariesController : ControllerBase
    {
        private readonly ILibraryService _libraryService;

        public LibrariesController(ILibraryService libraryService)
        {
            _libraryService = libraryService;
        }
        [HttpPost("add/{gameId}")]
        public async Task<IActionResult> AddToLibrary(int gameId)
        {
            
            int userId = User.GetUserId();

            // İşlemi Manager'a devrediyoruz
            await _libraryService.AddToLibraryAsync(userId, gameId);

            return Ok(new { message = "Oyun kütüphaneye başarıyla eklendi." });
        }
        [HttpGet("my-library")]
        public async Task<IActionResult> GetMyLibrary()
        {
            int userId = User.GetUserId();
            var library = await _libraryService.GetUserLibraryAsync(userId);

            return Ok(library);
        }
        [HttpPost("rate")]
        public async Task<IActionResult> RateGame([FromBody] GameRateDto gameRateDto)
        {
            int userId = User.GetUserId();

            await _libraryService.RateGameAsync(userId, gameRateDto.GameId, gameRateDto.Rating);

            return Ok(new { message = "Oyun başarıyla puanlandı." });
        }
    }
}
