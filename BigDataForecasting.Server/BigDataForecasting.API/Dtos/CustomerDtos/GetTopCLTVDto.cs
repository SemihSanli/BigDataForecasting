namespace BigDataForecasting.API.Dtos.CustomerDtos
{
    public class GetTopCLTVDto
    {
        public int TotalCustomerCount { get; set; }
        public int VipCount { get; set; }
        public int LoyalCount { get; set; }
        public int PotentialCount { get; set; }

        // Her kategoriden en iyi 3-5 kişiyi direkt buraya gömelim
        public List<AdminCLTVResultDto> TopVips { get; set; }
        public List<AdminCLTVResultDto> TopPotentialCustomers { get; set; }
    }
}
