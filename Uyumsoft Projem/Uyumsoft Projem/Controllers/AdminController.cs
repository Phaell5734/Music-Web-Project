using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uyumsoft_Projem.Models;
using Uyumsoft_Projem.ViewModels;

namespace Uyumsoft_Projem.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly MusicPlatformContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminController(MusicPlatformContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private async Task<bool> IsAdminAsync()
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            return user?.Roles.Any(r => r.RoleName == "Admin") == true;
        }

        public async Task<IActionResult> Index()
        {
            if (!await IsAdminAsync()) return Forbid();

            var totalUsers = await _context.Users.CountAsync(u => u.Status != "Banned"); 
            var totalActiveUsers = await _context.Users.CountAsync(u => u.Status == "Active");
            var totalSongs = await _context.Songs.CountAsync();
            var totalRevenue = await _context.BalanceTransactions
                .Where(bt => bt.Type == "Commission")
                .SumAsync(bt => bt.Amount ?? 0);
            var totalPlays = await _context.Songs.SumAsync(s => s.ClickTimes ?? 0);

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalSongs = totalSongs,
                TotalRevenue = totalRevenue,
                TotalPlays = totalPlays,
                MonthlyReports = await GetMonthlyReportsAsync(),
                RecentTransactions = await GetRecentTransactionsAsync(),
                TopSongs = await GetTopSongsAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Users()
        {
            if (!await IsAdminAsync()) return Forbid();

            var users = await _context.Users
                .Include(u => u.Roles)
                .Include(u => u.Subscriptions)
                .Include(u => u.Songs)
                .Where(u => u.Status != "Banned") 
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserStatus(int userId, string status)
        {
            try
            {
                if (!await IsAdminAsync())
                {
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                var validStatuses = new[] { "Active", "Banned" }; 
                if (!validStatuses.Contains(status))
                {
                    return Json(new { success = false, message = "Invalid status." });
                }

                user.Status = status;
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"User {user.UserName} status updated to {status}."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"An error occurred: {ex.Message}"
                });
            }
        }

        public async Task<IActionResult> Songs()
        {
            if (!await IsAdminAsync()) return Forbid();

            var songs = await _context.Songs
                .Include(s => s.Artists)
                .OrderByDescending(s => s.ClickTimes)
                .ToListAsync();

            return View(songs);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSong(int songId)
        {
            try
            {
                if (!await IsAdminAsync())
                {
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                var song = await _context.Songs
                    .Include(s => s.UserSongs)
                    .Include(s => s.PlaylistSongs)
                    .Include(s => s.Artists)
                    .FirstOrDefaultAsync(s => s.SongId == songId);

                if (song == null)
                {
                    return Json(new { success = false, message = "Song not found." });
                }

                _context.UserSongs.RemoveRange(song.UserSongs);
                _context.PlaylistSongs.RemoveRange(song.PlaylistSongs);
                
                song.Artists.Clear();

                DeletePhysicalFile(song.FilePath);
                DeletePhysicalFile(song.ImagePath);

                _context.Songs.Remove(song);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Song {song.Title} has been deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"An error occurred while deleting the song: {ex.Message}"
                });
            }
        }

        public async Task<IActionResult> MonthlyReports()
        {
            if (!await IsAdminAsync()) return Forbid();

            var reports = await _context.MonthlyReports
                .Include(mr => mr.Artist)
                .Where(mr => mr.Artist.Status != "Banned") 
                .OrderByDescending(mr => mr.ReportMonth)
                .ToListAsync();

            return View(reports);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateMonthlyReport(string month)
        {
            if (!await IsAdminAsync()) return Forbid();

            await GenerateMonthlyReportAsync(month);
            return Json(new { success = true, message = $"Monthly report for {month} has been generated." });
        }

        [HttpPost]
        public async Task<IActionResult> DistributeEarnings(string month)
        {
            if (!await IsAdminAsync()) return Forbid();

            await DistributeMonthlyEarningsAsync(month);
            return Json(new { success = true, message = $"Earnings for {month} have been distributed." });
        }

        private async Task<List<MonthlyReport>> GetMonthlyReportsAsync()
        {
            return await _context.MonthlyReports
                .Include(mr => mr.Artist)
                .Where(mr => mr.Artist.Status != "Banned")
                .OrderByDescending(mr => mr.ReportMonth)
                .Take(5)
                .ToListAsync();
        }

        private async Task<List<BalanceTransaction>> GetRecentTransactionsAsync()
        {
            return await _context.BalanceTransactions
                .Include(bt => bt.User)
                .Where(bt => bt.User.Status != "Banned")
                .OrderByDescending(bt => bt.TransactionDate)
                .Take(10)
                .ToListAsync();
        }

        private async Task<List<Song>> GetTopSongsAsync()
        {
            return await _context.Songs
                .Include(s => s.Artists.Where(a => a.Status != "Banned"))
                .OrderByDescending(s => s.ClickTimes)
                .Take(10)
                .ToListAsync();
        }

        private void DeletePhysicalFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            var fullPath = Path.Combine(_env.WebRootPath,
                relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        private async Task GenerateMonthlyReportAsync(string month)
        {
            var artists = await _context.Users
                .Include(u => u.Songs)
                .Where(u => u.Songs.Any() && u.Status != "Banned")
                .ToListAsync();
            
            var totalPlays = await _context.Songs.SumAsync(s => s.ClickTimes ?? 0);
            var (startDate, endDate) = GetMonthDateRange(month);
            var totalMonthlyRevenue = await CalculateMonthlyRevenueAsync(startDate, endDate);

            foreach (var artist in artists)
            {
                var artistPlays = artist.Songs.Sum(s => s.ClickTimes ?? 0);
                var playRatio = totalPlays > 0 ? (decimal)artistPlays / totalPlays : 0;

                var artistRevenue = totalMonthlyRevenue * playRatio;
                var platformCommission = artistRevenue * 0.15m;
                var artistPayout = artistRevenue * 0.85m;

                var existingReport = await _context.MonthlyReports
                    .FirstOrDefaultAsync(mr => mr.ArtistId == artist.UserId && mr.ReportMonth == month);

                if (existingReport == null)
                {
                    _context.MonthlyReports.Add(new MonthlyReport
                    {
                        ArtistId = artist.UserId,
                        ReportMonth = month,
                        TotalPlays = artistPlays,
                        TotalRevenue = artistRevenue,
                        PlatformCommission = platformCommission,
                        ArtistPayout = artistPayout
                    });
                }
                else
                {
                    existingReport.TotalPlays = artistPlays;
                    existingReport.TotalRevenue = artistRevenue;
                    existingReport.PlatformCommission = platformCommission;
                    existingReport.ArtistPayout = artistPayout;
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task DistributeMonthlyEarningsAsync(string month)
        {
            var reports = await _context.MonthlyReports
                .Include(mr => mr.Artist)
                .Where(mr => mr.ReportMonth == month && mr.Artist.Status != "Banned")
                .ToListAsync();

            var admin = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Roles.Any(r => r.RoleName == "Admin"));

            foreach (var report in reports)
            {
                if (report.Artist != null && report.ArtistPayout > 0)
                {
                    report.Artist.Balance = (report.Artist.Balance ?? 0) + report.ArtistPayout;

                    _context.BalanceTransactions.Add(new BalanceTransaction
                    {
                        UserId = report.Artist.UserId,
                        Amount = report.ArtistPayout,
                        Type = "Monthly_Payout",
                        Description = $"Monthly earnings for {month}",
                        TransactionDate = DateTime.Now
                    });

                    if (admin != null && report.PlatformCommission > 0)
                    {
                        admin.Balance = (admin.Balance ?? 0) + report.PlatformCommission;

                        _context.BalanceTransactions.Add(new BalanceTransaction
                        {
                            UserId = admin.UserId,
                            Amount = report.PlatformCommission,
                            Type = "Commission",
                            Description = $"Platform commission for {month} from {report.Artist.UserName}",
                            TransactionDate = DateTime.Now
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        private (DateTime startDate, DateTime endDate) GetMonthDateRange(string month)
        {
            var startDate = DateTime.ParseExact(month + "-01", "yyyy-MM-dd", null);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            return (startDate, endDate);
        }

        private async Task<decimal> CalculateMonthlyRevenueAsync(DateTime startDate, DateTime endDate)
        {
            var monthlySubscriptionRevenue = await _context.Subscriptions
                .Include(s => s.User)
                .Where(s => s.StartDate >= startDate && s.StartDate <= endDate && s.User.Status != "Banned")
                .SumAsync(s => s.MonthlyFee ?? 0);

            return monthlySubscriptionRevenue;
        }
    }
}