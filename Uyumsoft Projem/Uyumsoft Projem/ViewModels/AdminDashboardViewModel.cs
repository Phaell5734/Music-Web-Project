using Uyumsoft_Projem.Models;

namespace Uyumsoft_Projem.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalSongs { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalPlays { get; set; }
        public List<MonthlyReport> MonthlyReports { get; set; } = new();
        public List<BalanceTransaction> RecentTransactions { get; set; } = new();
        public List<Song> TopSongs { get; set; } = new();
    }
}
