using BigDataForecasting.API.Services.BaseServices.SaleServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BigDataForecasting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _saleService;

        public SalesController(ISaleService saleService)
        {
            _saleService = saleService;
        }

        [HttpGet("GetAllSalesWithDetails")]
        public async Task<IActionResult> GetAllSalesWithDetails([FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 50)
        {
            var sales = await _saleService.GetAllSalesAsync(pageNumber,pageSize);
            return Ok(sales);
        }
        [HttpGet("TotalRevenue")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            var totalRevenue = await _saleService.GetTotalRevenueAsync();
            return Ok(totalRevenue);
        }
        [HttpGet("LastYearSalesReport")]
        public async Task<IActionResult> GetLastYearSalesReport()
        {
            var report = await _saleService.GetLastYearSalesReportAsync();
            return Ok(report);

        }
        [HttpGet("MonthlySales")]
        public async Task<IActionResult> GetMonthlySales()
        {
            var monthlySales = await _saleService.GetMonthlySalesAsync();
            return Ok(monthlySales);
        }
        [HttpGet("Top5BestSellingGames")]
        public async Task<IActionResult> GetTop5BestSellingGames()
        {
            var top5Games = await _saleService.GetTop5BestSellingGamesAsync();
            return Ok(top5Games);
        }
        [HttpGet("SalesDistributionByGenre")]
        public async Task<IActionResult> GetSalesDistributionByGenre()
        {
            var distribution = await _saleService.GetSalesDistributionByGenreAsync();
            return Ok(distribution);
        }
    }
}
