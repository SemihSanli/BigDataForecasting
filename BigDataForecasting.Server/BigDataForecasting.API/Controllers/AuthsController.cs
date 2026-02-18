using BigDataForecasting.API.Dtos.AuthDtos;
using BigDataForecasting.API.Services.BaseServices.AuthServices;
using Microsoft.AspNetCore.Mvc;

namespace BigDataForecasting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthsController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthsController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Yeni kullanıcı kaydı
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var result = await _authService.RegisterAsync(dto);
                return Ok(result);
            }
            catch (Exception ex) when (ex.Message.Contains("already exists"))
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Giriş yap
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);
                return Ok(result);
            }
            catch (Exception ex) when (ex.Message.Contains("Invalid") || ex.Message.Contains("hatalı") || ex.Message.Contains("inactive"))
            {
                if (ex.Message.Contains("inactive"))
                    return StatusCode(403, new { message = ex.Message });
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
