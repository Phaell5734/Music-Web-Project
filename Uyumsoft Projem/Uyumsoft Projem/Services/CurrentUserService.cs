using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Uyumsoft_Projem.Models;

namespace Uyumsoft_Projem.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _account;
        private readonly MusicPlatformContext _context;
        public CurrentUserService(IHttpContextAccessor account, MusicPlatformContext context)
        {
            _account = account; 
            _context = context;
        }

        public User? Current { get; private set; }

        public async Task EnsureLoadedAsync()
        {
            if (Current != null || !_account.HttpContext!.User.Identity!.IsAuthenticated)
                return;

            var uid = int.Parse(_account.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            Current = await _context.Users
                .Include(u => u.Subscriptions)
                .Include(u => u.Roles)
                .Include(u => u.UserSongs)  
                .FirstOrDefaultAsync(u => u.UserId == uid);
        }

        public bool HasActiveSubscription()
        {
            var now = DateTime.Now;
            return Current?.Subscriptions.Any(s => s.Status == "Active" &&
                                                   s.StartDate <= now &&
                                                   s.EndDate >= now) == true;
        }

        public bool IsAdmin()
        {
            return Current?.Roles.Any(r => r.RoleName == "Admin") == true;
        }
    }
}
