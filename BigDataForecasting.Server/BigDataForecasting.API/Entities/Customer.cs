using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BigDataForecasting.API.Entities
{
    public class Customer
    {
      
        public int CustomerId { get; set; }

        public string UserName { get; set; }

      
        public string Email { get; set; }

        // 2. Güvenlik ve Yetki
       
        public string PasswordHash { get; set; }

       
      
        public string Role { get; set; } = "Member"; // Default 'Member'

        public bool IsActive { get; set; } = true; // Default 1 (True)

      
     
        public string? FirstName { get; set; } // Soru işareti (?) NULL olabilir demek

      
        public string? LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

      
        public string? City { get; set; }

      
        public string CountryCode { get; set; } = "TR"; // Default 'TR'

       
        public long? SteamId { get; set; } // BIGINT karşılığı long'dur

      
        public string? ProfileImageUrl { get; set; }

        
        public decimal WalletBalance { get; set; } = 0.00m;

        // 5. Zaman Damgaları
        public DateTime CreatedDate { get; set; } = DateTime.Now; // Default GETDATE()

        public DateTime? LastLoginDate { get; set; }

        // Navigation Property (İleride Sales tablosu ile ilişki için)
        // public ICollection<Sale> Sales { get; set; }
        public ICollection<Sale> Sales { get; set; }
    }
}
