using BigDataForecasting.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigDataForecasting.API.Context.Configuration
{
    public class GameDetailConfiguration : IEntityTypeConfiguration<GameDetail>
    {
        public void Configure(EntityTypeBuilder<GameDetail> builder)
        {
            builder.HasKey(gd => gd.GameDetailId);

            builder.Property(gd => gd.Developer)
                .HasMaxLength(100);

            // 1:1 ilişki - Her Game'in bir GameDetail'ı olabilir
            builder.HasOne(gd => gd.Game)
                .WithOne(g => g.GameDetail)
                .HasForeignKey<GameDetail>(gd => gd.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
