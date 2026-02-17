namespace BigDataForecasting.API.Entities
{
    public class UserActivity
    {
        public int UserActivityId { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public string ActivityType { get; set; } // "Login", "AddToLibrary", "Rate", "Wishlist"
        public DateTime ActivityDate { get; set; } = DateTime.Now;
    }
}
