using BigDataForecasting.API.Services.MLServices;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BigDataForecasting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        private readonly IAITrainerService _aiTrainerService;

        public RecommendationsController(IAITrainerService aiTrainerService)
        {
            _aiTrainerService = aiTrainerService;
        }

        [HttpPost("train-recommendations")]
        public IActionResult TrainRecommendationModel()
        {
            BackgroundJob.Enqueue<IAITrainerService>(x => x.TrainRecommendationModelAsync());
            return Ok("Öneri sistemi AI modeli başarıyla eğitildi ve zip olarak kaydedildi.");
        }

        [HttpGet("recommendations/{customerId}")]
        public async Task<IActionResult> GetRecommendations(int customerId, [FromQuery] int topN = 5)
        {
            var recommendations = await _aiTrainerService.GetGameRecommendationsForUserAsync(customerId, topN);
            return Ok(recommendations);
        }
        [HttpGet("dashboard-recommendations")]
        public async Task<IActionResult> GetDashboardRecommendations()
        {
            var result = await _aiTrainerService.GetRandomCustomerRecommendationsAsync();
            return Ok(result);
        }
    }
}
