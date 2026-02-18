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

      
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetGameDetail(int id)
        {
            var game = await _gameService.GetGameDetailAsync(id);
            if (game == null)
                return NotFound();
            return Ok(game);
        }
    }
}
