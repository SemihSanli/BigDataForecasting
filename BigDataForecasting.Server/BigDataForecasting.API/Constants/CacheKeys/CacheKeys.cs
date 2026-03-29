namespace BigDataForecasting.API.Constants.CacheKeys
{
    public class CacheKeys
    {
        public static class Dashboard
        {
            public const string TopCltv = "dashboard:top_cltv";
            public const string GlobeNodes = "dashboard:globe_nodes";
            public const string MonthlySales = "dashboard:monthly_sales";
        }

        // Müşteri ile ilgili anahtarlar
        public static class Customer
        {
            public const string AllActive = "customer:all_active";

            // PARAMETRİK KEY: ID'ye göre cachelemek için metot kullanıyoruz
            public static string Details(int id) => $"customer:{id}:details";
            public static string Wishlist(int id) => $"customer:{id}:wishlist";
        }

        // ML Modelleri ile ilgili anahtarlar
        public static class ML
        {
            public const string ChurnModel = "ml:churn_model";
            public static string Recommendations(int userId) => $"ml:recommendations:{userId}";

            public const string AllCLTVPredictions = "ml:all_cltv";
            public const string RandomRecommendations = "ml:dashboard_random_recs";
            public const string TopRiskyCustomers = "ml:top_risky_customers";
            public const string RevenueForecast = "ml:revenue_forecast";
            public static string GameRecommendations(int userId) => $"ml:user_recs:{userId}";
        }

        public static class SaleKeys
        {
            public const string LastYearReport = "sale:last_year_report";
            public const string MonthlySales = "sale:monthly_sales";
            public const string DistributionByGenre = "sale:distribution_by_genre";
            public const string Top5Games = "sale:top_5_games";
            public const string TotalRevenue = "sale:total_revenue";
            public static string OwnedGames(int customerId) => $"sale:owned_games:{customerId}";
        }

        public static class LibraryKeys
        {
            public static string UserLibrary(int customerId) => $"library:user:{customerId}";
        }

        public static class WishlistKeys
        {
            public static string UserWishlist(int customerId) => $"wishlist:user:{customerId}";
        }

        public static class GlobeKeys
        {
            public const string Nodes = "globe:nodes";
        }

        public static class GameKeys
        {
            public const string All = "game:all"; // Bunu ekledik
            public const string AllBasic = "game:all_basic";
            public const string AllWithCategory = "game:all_with_category"; // Bunu ekledik
            public static string Detail(int gameId) => $"game:detail:{gameId}";
        }
    }
}
