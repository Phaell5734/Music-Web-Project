using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Uyumsoft_Projem.Models;

namespace Uyumsoft_Projem.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        private readonly MusicPlatformContext _context;
        public LibraryController(MusicPlatformContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            var songs = await _context.UserSongs
                                  .Where(us => us.UserId == user.UserId)
                                  .Select(us => us.Song)
                                  .ToListAsync();

            return View(songs);   
        }
    }

}
