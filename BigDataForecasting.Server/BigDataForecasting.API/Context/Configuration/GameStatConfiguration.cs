using BigDataForecasting.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigDataForecasting.API.Context.Configuration
{
    public class GameStatConfiguration : IEntityTypeConfiguration<GameStat>
    {
        public void Configure(EntityTypeBuilder<GameStat> builder)
        {
            builder.HasKey(gs => gs.GameStatId);

            builder.Property(gs => gs.AverageRating)
                .HasDefaultValue(0);

            builder.Property(gs => gs.TotalLibraryAdds)
                .HasDefaultValue(0);

            // 1:1 ilişki - Her Game'in bir GameStat'ı olabilir
            builder.HasOne(gs => gs.Game)
                .WithOne(g => g.GameStat)
                .HasForeignKey<GameStat>(gs => gs.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
