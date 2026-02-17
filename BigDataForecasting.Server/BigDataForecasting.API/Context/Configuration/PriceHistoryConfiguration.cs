using BigDataForecasting.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigDataForecasting.API.Context.Configuration
{
    public class PriceHistoryConfiguration : IEntityTypeConfiguration<PriceHistory>
    {
        public void Configure(EntityTypeBuilder<PriceHistory> builder)
        {
            builder.HasKey(ph => ph.PriceHistoryId);

            builder.Property(ph => ph.Price)
                .HasPrecision(18, 2);

            builder.Property(ph => ph.DiscountPercent)
                .HasPrecision(5, 2);

            // Bir PriceHistory -> Bir Game'e ait
            builder.HasOne(ph => ph.Game)
                .WithMany(g => g.PriceHistories)
                .HasForeignKey(ph => ph.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
