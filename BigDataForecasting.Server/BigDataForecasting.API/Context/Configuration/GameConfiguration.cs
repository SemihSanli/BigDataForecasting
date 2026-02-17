using BigDataForecasting.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigDataForecasting.API.Context.Configuration
{
    public class GameConfiguration:IEntityTypeConfiguration<Game>
    {
        public void Configure(EntityTypeBuilder<Game> builder)
        {
            builder.HasKey(g => g.GameId);

            builder.Property(g => g.GameName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(g => g.Price)
                .HasPrecision(18, 2);

            builder.Property(g => g.Genre)
                .HasMaxLength(50);
        }
    }
}
