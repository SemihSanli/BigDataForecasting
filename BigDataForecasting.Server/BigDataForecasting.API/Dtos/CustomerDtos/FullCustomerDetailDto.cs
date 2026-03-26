namespace BigDataForecasting.API.Dtos.CustomerDtos
{
    public class FullCustomerDetailDto
    {
        public int CustomerId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? FullName { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string CountryCode { get; set; }
        public decimal WalletBalance { get; set; }
        public DateTime CreatedDate { get; set; }

        // --- İSTATİSTİKLER (Dashboard Rozetleri İçin) ---
        public int OwnedGameCount { get; set; }
        public int WishlistGameCount { get; set; }
        public double TotalPlayTimeHours { get; set; }

        // --- LİSTELER (Akordeon veya Detay Tablosu İçin) ---
        public List<CustomerLibraryItemDto> Library { get; set; } = new();
        public List<CustomerWishListItemDto> Wishlist { get; set; } = new();
    }
}
