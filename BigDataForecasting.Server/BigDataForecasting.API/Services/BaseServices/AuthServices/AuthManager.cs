using BigDataForecasting.API.Context.ForecastingDb;
using BigDataForecasting.API.Dtos.AuthDtos;
using BigDataForecasting.API.Entities;
using BigDataForecasting.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BigDataForecasting.API.Services.BaseServices.AuthServices
{
    public class AuthManager : IAuthService
    {
        private readonly IGenericRepository<Customer> _customerRepository;
        private readonly AppDbContext _appDbContext;
        private readonly ITokenService _tokenService;


        public AuthManager(IGenericRepository<Customer> customerRepository, AppDbContext appDbContext, ITokenService tokenService)
        {
            _customerRepository = customerRepository;
            _appDbContext = appDbContext;
            _tokenService = tokenService;
        }

        public async Task<ResponseDto> LoginAsync(LoginDto loginDto)
        {
            var customer = await _customerRepository.Where(c => c.Email == loginDto.Email, tracking: true).FirstOrDefaultAsync();
            if (customer == null)
            {
                throw new Exception("Invalid email or password.");
            }
            if (!customer.IsActive)
            {
                               throw new Exception("Account is inactive.");

            }
            var passwordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, customer.PasswordHash);
            if (!passwordValid)
                throw new Exception("Email veya şifre hatalı.");
            customer.LastLoginDate = DateTime.UtcNow;
            await _appDbContext.SaveChangesAsync();

            var token = _tokenService.GenerateToken(customer);
            return new ResponseDto
            {
                Token = token,
                UserName = customer.UserName,
                Role = customer.Role
            };
        }

        public async Task<ResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var emailExists = await _customerRepository.AnyAsync(c=>c.Email == registerDto.Email);
            if(emailExists)
            {
                throw new Exception("Email already exists.");
            }
            var userNameExists = await _customerRepository.AnyAsync(c=>c.UserName == registerDto.UserName);
            if (userNameExists)
            {
                throw new Exception("Username already exists.");
            }
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            var customer = new Customer
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                PasswordHash = hashedPassword,
                Role = "Member",
                CreatedDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow
            };
            await _customerRepository.AddAsync(customer);
            await _appDbContext.SaveChangesAsync();

            var token = _tokenService.GenerateToken(customer);

            return new ResponseDto
            {
                Token = token,
                UserName = customer.UserName,
                Role = customer.Role
            };
        }
    }
}
