using BigDataForecasting.API.Services.BaseServices.GlobeAnalyticsServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BigDataForecasting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IGlobeAnalyticsService _globeAnalyticsService;

        public AnalyticsController(IGlobeAnalyticsService globeAnalyticsService)
        {
            _globeAnalyticsService = globeAnalyticsService;
        }

        [HttpGet("global-nodes")]
        public async Task<IActionResult> GetGlobalNodes()
        {
            var result = await _globeAnalyticsService.GetGlobalNodesAsync();
            return Ok(result);
        }
    }
}
