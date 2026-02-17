using BigDataForecasting.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigDataForecasting.API.Context.Configuration
{
    public class GameCategoryConfiguration:IEntityTypeConfiguration<GameCategory>
    {
        public void Configure(EntityTypeBuilder<GameCategory> builder) 
        {
            builder.HasKey(c => c.GameCategoryId);

            builder.Property(c => c.GameCategoryName)
                .IsRequired()
                .HasMaxLength(50);

            // N-N İlişki Yapılandırması
            builder.HasMany(c => c.Games)
                .WithMany(g => g.GameCategories)
                .UsingEntity(j => j.ToTable("GameCategoryMappings")); // Ara tablonun ad
        }
    }
}
