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
                //Client'e token'ın  cookie üzerinden gitmesini sağlamak için CookieOptions ekledim.
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddMinutes(60)
                };
                Response.Cookies.Append("auth_token", result.Token, cookieOptions);
                return Ok(new
                {
                    Username = result.UserName,
                    Role = result.Role,
                    Message = "Giriş başarılı,token güvenli bir şekilde cookie'ye yazıldı"
                });
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
