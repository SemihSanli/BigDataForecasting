using BigDataForecasting.API.Dtos.AuthDtos;

namespace BigDataForecasting.API.Services.BaseServices.AuthServices
{
    public interface IAuthService
    {
        Task<ResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<ResponseDto> LoginAsync(LoginDto loginDto);
    }
}
