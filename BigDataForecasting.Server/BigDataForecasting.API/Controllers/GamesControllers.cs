using BigDataForecasting.API.Services.BaseServices.GameServices;
using Microsoft.AspNetCore.Mvc;

namespace BigDataForecasting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesControllers : ControllerBase
    {
        private readonly IGameService _gameService;

        public GamesControllers(IGameService gameService)
        {
            _gameService = gameService;
        }

      
        [HttpGet]
        public async Task<IActionResult> GetAllGames()
        {
            var games = await _gameService.GetAllGameAsync();
            return Ok(games);
        }
        [HttpGet("GamesWithCategories")]
        public async Task<IActionResult> GetGamesWithCategories()
        {
            var gamesWithCategories = await _gameService.GetGamesWithCategoryAsync();   
            return Ok(gamesWithCategories);
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetGameDetail(int id)
        {
            var game = await _gameService.GetGameDetailAsync(id);
            if (game == null)
                return NotFound();
            return Ok(game);
        }
        [HttpGet("GetAllGamesWithDetails")]
        public async Task<IActionResult> GetGames(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null)
        {
            // Servise parametreleri yolla, düz List<FullGameDetailDto> gelsin
            var games = await _gameService.GetAllGamesWithFullDetailsAsync(pageNumber, pageSize, searchTerm, sortBy);

            // 200 OK statüsü ile frontend'e JSON olarak bas
            return Ok(games);
        }
    }
}
