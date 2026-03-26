using BigDataForecasting.API.Services.BaseServices.CustomerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BigDataForecasting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("ActiveCustomers")]
        public async Task<IActionResult> GetActiveCustomers()
        {
            var activeCustomersCount = await _customerService.GetActiveUsersAsync();
            return Ok(activeCustomersCount);
        }
        [HttpGet("WalletBalance")]
        public async Task<IActionResult> GetWalletBalance()
        {
            var walletBalance = await _customerService.GetWalletBalanceAsync();
            return Ok(walletBalance);
        }

        [HttpGet("customer-full-details")]
        public async Task<IActionResult> GetCustomersWithDetails(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null)
        {
           
            var customers = await _customerService.GetAllCustomersWithFullDetailsAsync(pageNumber, pageSize, searchTerm, sortBy);

          
            return Ok(customers);
        }
    }
}
