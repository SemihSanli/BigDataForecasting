using BigDataForecasting.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigDataForecasting.API.Context.Configuration
{
    public class CustomerConfiguration:IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(c => c.CustomerId);

            // Benzersiz Alanlar (Unique Index)
            builder.HasIndex(c => c.UserName).IsUnique();
            builder.HasIndex(c => c.Email).IsUnique();

            // Property Ayarları
            builder.Property(c => c.UserName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.WalletBalance)
                .HasPrecision(18, 2) 
                .HasDefaultValue(0);
        }
    }
    
}
