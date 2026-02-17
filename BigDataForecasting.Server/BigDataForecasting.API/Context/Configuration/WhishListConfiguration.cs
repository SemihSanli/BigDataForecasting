using BigDataForecasting.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigDataForecasting.API.Context.Configuration
{
    public class WhishListConfiguration : IEntityTypeConfiguration<WhishList>
    {
        public void Configure(EntityTypeBuilder<WhishList> builder)
        {
            builder.HasKey(w => w.WishlistId);

            // Bir WhishList -> Bir Customer'a ait
            builder.HasOne(w => w.Customer)
                .WithMany(c => c.WhishLists)
                .HasForeignKey(w => w.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bir WhishList -> Bir Game'e ait
            builder.HasOne(w => w.Game)
                .WithMany(g => g.WhishLists)
                .HasForeignKey(w => w.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
