using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uyumsoft_Projem.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Uyumsoft_Projem.Controllers
{
    [Authorize]
    public class ArtistController : Controller
    {
        private readonly MusicPlatformContext _context;

        public ArtistController(MusicPlatformContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string artistName)
        {
            if (string.IsNullOrEmpty(artistName))
            {
                return NotFound();
            }

            var artist = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == artistName);

            if (artist == null)
            {
                return NotFound();
            }

            var songs = await _context.Songs
                .Include(s => s.Artists)
                .Where(s => s.Artists.Any(a => a.UserId == artist.UserId))
                .ToListAsync();

            ViewBag.IsFollowing = User.Identity.IsAuthenticated && await _context.Users
                .Include(u => u.Followers)
                .AnyAsync(u => u.UserName == User.Identity.Name && u.Followers.Any(f => f.UserId == artist.UserId));

            ViewBag.Artist = artist;
            return View(songs);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Follow(string artistName)
        {
            if (string.IsNullOrEmpty(artistName))
            {
                return NotFound();
            }

            var artist = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == artistName);

            var follower = await _context.Users
                .Include(u => u.Followers)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (artist == null || follower == null)
            {
                return NotFound();
            }

            if (follower.Followers.Any(f => f.UserId == artist.UserId))
            {
                var toRemove = follower.Followers.FirstOrDefault(f => f.UserId == artist.UserId);
                if (toRemove != null)
                {
                    follower.Followers.Remove(toRemove);
                }
            }
            else
            {
                follower.Followers.Add(artist);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { artistName = artistName });
        }

        [Authorize]
        public async Task<IActionResult> Following()
        {
            var user = await _context.Users
                .Include(u => u.Followers)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user == null)
            {
                return NotFound();
            }

            return View(user.Followers.ToList());
        }
    }
}
