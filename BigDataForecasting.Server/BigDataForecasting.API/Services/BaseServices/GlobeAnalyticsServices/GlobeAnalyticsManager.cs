using BigDataForecasting.API.Dtos.GlobeDtos;
using BigDataForecasting.API.Services.BaseServices.CustomerServices;

namespace BigDataForecasting.API.Services.BaseServices.GlobeAnalyticsServices
{
    public class GlobeAnalyticsManager : IGlobeAnalyticsService
    {
        private readonly ICustomerService _customerService;

        public GlobeAnalyticsManager(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<GlobeNodeResultDto> GetGlobalNodesAsync()
        {
            var result = new GlobeNodeResultDto();

            // Sorumlusundan tüm müşteri istatistiklerini çek!
            var countryStats = await _customerService.GetCustomerCountByCountryAsync();

            // 62 Ülkelik Dev Koordinat Ağı (Senin CSV'ne göre özel çıkarıldı)
            var coordinates = new Dictionary<string, (double Lat, double Lng, string CityName)>
            {
                { "TR", (41.0082, 28.9784, "Istanbul") },
                { "IT", (41.9028, 12.4964, "Rome") },
                { "DE", (52.5200, 13.4050, "Berlin") },
                { "CA", (43.6510, -79.3470, "Toronto") },
                { "GR", (37.9838, 23.7275, "Athens") },
                { "UA", (50.4501, 30.5234, "Kyiv") },
                { "KE", (-1.2921, 36.8219, "Nairobi") },
                { "IN", (28.6139, 77.2090, "New Delhi") },
                { "PH", (14.5995, 120.9842, "Manila") },
                { "IE", (53.3498, -6.2603, "Dublin") },
                { "CH", (47.3769, 8.5417, "Zurich") },
                { "GE", (41.7151, 44.8271, "Tbilisi") },
                { "CN", (39.9042, 116.4074, "Beijing") },
                { "TH", (13.7563, 100.5018, "Bangkok") },
                { "SA", (24.7136, 46.6753, "Riyadh") },
                { "ES", (40.4168, -3.7038, "Madrid") },
                { "BG", (42.6977, 23.3219, "Sofia") },
                { "BE", (50.8503, 4.3517, "Brussels") },
                { "PK", (33.6844, 73.0479, "Islamabad") },
                { "DK", (55.6761, 12.5683, "Copenhagen") },
                { "CZ", (50.0755, 14.4378, "Prague") },
                { "EG", (30.0444, 31.2357, "Cairo") },
                { "GB", (51.5074, -0.1278, "London") },
                { "KW", (29.3759, 47.9774, "Kuwait City") },
                { "CO", (4.7110, -74.0721, "Bogota") },
                { "FR", (48.8566, 2.3522, "Paris") },
                { "NO", (59.9139, 10.7522, "Oslo") },
                { "ZA", (-33.9249, 18.4241, "Cape Town") },
                { "US", (40.7128, -74.0060, "New York") },
                { "MA", (33.5731, -7.5898, "Casablanca") },
                { "QA", (25.2854, 51.5310, "Doha") },
                { "RO", (44.4268, 26.1025, "Bucharest") },
                { "RU", (55.7558, 37.6173, "Moscow") },
                { "JP", (35.6762, 139.6503, "Tokyo") },
                { "MX", (19.4326, -99.1332, "Mexico City") },
                { "AE", (25.2048, 55.2708, "Dubai") },
                { "AU", (-33.8688, 151.2093, "Sydney") },
                { "CL", (-33.4489, -70.6693, "Santiago") },
                { "NL", (52.3676, 4.9041, "Amsterdam") },
                { "HU", (47.4979, 19.0402, "Budapest") },
                { "FI", (60.1695, 24.9354, "Helsinki") },
                { "GH", (5.6037, -0.1870, "Accra") },
                { "VN", (21.0285, 105.8542, "Hanoi") },
                { "PL", (52.2297, 21.0122, "Warsaw") },
                { "ID", (-6.2088, 106.8456, "Jakarta") },
                { "AR", (-34.6037, -58.3816, "Buenos Aires") },
                { "TN", (36.8065, 10.1815, "Tunis") },
                { "AZ", (40.4093, 49.8671, "Baku") },
                { "SE", (59.3293, 18.0686, "Stockholm") },
                { "MY", (3.1390, 101.6869, "Kuala Lumpur") },
                { "BR", (-23.5505, -46.6333, "Sao Paulo") },
                { "HK", (22.3193, 114.1694, "Hong Kong") },
                { "SG", (1.3521, 103.8198, "Singapore") },
                { "NG", (6.5244, 3.3792, "Lagos") },
                { "AT", (48.2082, 16.3738, "Vienna") },
                { "PT", (38.7223, -9.1393, "Lisbon") },
                { "KR", (37.5665, 126.9780, "Seoul") },
                { "PE", (-12.0464, -77.0428, "Lima") },
                { "NZ", (-41.2865, 174.7762, "Wellington") },
                { "LK", (6.9271, 79.8612, "Colombo") },
                { "IR", (35.6892, 51.3890, "Tehran") },
                { "BD", (23.8103, 90.4125, "Dhaka") }
            };

            // Eşleştirme ve Değer (Büyüklük) Ataması
            foreach (var stat in countryStats)
            {
                if (coordinates.TryGetValue(stat.CountryCode, out var coord))
                {
                    // Türkiye'de 317 kullanıcı var, diğerlerinde 10-20. 
                    // Küre üzerindeki çubukların/noktaların aşırı orantısız olmaması için normalize ediyoruz.
                    double normalizedValue = Math.Min((double)stat.CustomerCount / 100.0, 1.0);

                    result.Locations.Add(new GlobeLocationDto
                    {
                        City = coord.CityName,
                        Lat = coord.Lat,
                        Lng = coord.Lng,
                        Value = normalizedValue > 0.05 ? normalizedValue : 0.1,
                        CustomerCount = stat.CustomerCount
                    });
                }
            }

            // Veri Akışı Kavisleri (Türkiye'den Dünyaya Uçan Işınlar)
            var merkezUs = result.Locations.FirstOrDefault(l => l.City == "Istanbul");
            if (merkezUs != null)
            {
                foreach (var loc in result.Locations.Where(l => l.City != "Istanbul"))
                {
                    result.Arcs.Add(new GlobeArcDto
                    {
                        StartLat = merkezUs.Lat,
                        StartLng = merkezUs.Lng,
                        EndLat = loc.Lat,
                        EndLng = loc.Lng
                    });
                }
            }

            return result;
        }
    }
}
