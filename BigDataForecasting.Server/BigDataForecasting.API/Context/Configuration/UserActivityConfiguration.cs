using BigDataForecasting.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigDataForecasting.API.Context.Configuration
{
    public class UserActivityConfiguration : IEntityTypeConfiguration<UserActivity>
    {
        public void Configure(EntityTypeBuilder<UserActivity> builder)
        {
            builder.HasKey(ua => ua.UserActivityId);

            builder.Property(ua => ua.ActivityType)
                .IsRequired()
                .HasMaxLength(30);

            // Bir UserActivity -> Bir Customer'a ait
            builder.HasOne(ua => ua.Customer)
                .WithMany(c => c.UserActivities)
                .HasForeignKey(ua => ua.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
