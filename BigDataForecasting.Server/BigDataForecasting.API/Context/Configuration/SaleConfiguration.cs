using BigDataForecasting.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigDataForecasting.API.Context.Configuration
{
    public class SaleConfiguration:IEntityTypeConfiguration<Sale>
    {
        public void Configure(EntityTypeBuilder<Sale> builder)
        {
            builder.HasKey(s => s.SaleId);

            // Decimal Hassasiyetleri
            builder.Property(s => s.SoldPrice).HasPrecision(18, 2);

            // --- İLİŞKİLER ---

            // Bir Satış -> Bir Müşteriye aittir.
            builder.HasOne(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Cascade); // Müşteri silinirse geçmişi de silinsin

            // Bir Satış -> Bir Oyuna aittir.
            builder.HasOne(s => s.Game)
                .WithMany(g => g.Sales)
                .HasForeignKey(s => s.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
