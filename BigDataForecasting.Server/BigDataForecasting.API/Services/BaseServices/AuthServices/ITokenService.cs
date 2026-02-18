using BigDataForecasting.API.Entities;

namespace BigDataForecasting.API.Services.BaseServices.AuthServices
{
    public interface ITokenService
    {
        string GenerateToken(Customer customer);
    }
}
