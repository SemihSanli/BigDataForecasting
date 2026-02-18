using BigDataForecasting.API.Context.ForecastingDb;
using BigDataForecasting.API.Entities;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace BigDataForecasting.API.Services.BaseServices.AuthServices
{
    public class TokenManager : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(Customer customer)
        {
            var claims = new[]
            {
                new System.Security.Claims.Claim("CustomerId", customer.CustomerId.ToString()),
                new System.Security.Claims.Claim("UserName", customer.UserName),
                new System.Security.Claims.Claim("Email", customer.Email),
                new System.Security.Claims.Claim("Role", customer.Role)
            };
            var key = new SymmetricSecurityKey(
          Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    double.Parse(_configuration["JwtSettings:ExpireMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}

