using Uyumsoft_Projem.Models;

namespace Uyumsoft_Projem.ViewModels
{
    public class AdminAnalyticsViewModel
    {
        public Dictionary<string, int> MonthlyUserGrowth { get; set; } = new();
        public Dictionary<string, decimal> RevenueTrends { get; set; } = new();
        public Dictionary<string, int> PopularGenres { get; set; } = new();
        public List<User> TopArtists { get; set; } = new();
    }
}
