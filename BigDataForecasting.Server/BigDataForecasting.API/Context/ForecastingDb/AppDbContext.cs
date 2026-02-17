using BigDataForecasting.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigDataForecasting.API.Context.ForecastingDb
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        // DbSet'lerimizi tanımlayalım
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<GameCategory> GameCategories { get; set; }
        public DbSet<GameDetail> GameDetails { get; set; }
        public DbSet<GameStat> GameStats { get; set; }
        public DbSet<WhishList> WhishLists { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Konfigürasyon sınıflarımızı uygulayalım
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
