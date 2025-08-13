using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Uyumsoft_Projem.Models;
using Uyumsoft_Projem.Services;
using Uyumsoft_Projem.ViewModels;

namespace Uyumsoft_Projem.Controllers
{
    [Authorize] 
    public class SongController : Controller
    {
        private readonly MusicPlatformContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ICurrentUserService _currentUserService;

        public SongController(IWebHostEnvironment env, ICurrentUserService currentUserService)
        {
            _context = new MusicPlatformContext();
            _env = env;
            _currentUserService = currentUserService;
        }

        [AllowAnonymous] 
        public async Task<IActionResult> Index()
        {
            if (User.Identity?.Name == null)
            {
                var allSongs = await _context.SongsDetails.ToListAsync();
                ViewBag.AllSongs = allSongs;
                ViewBag.Current = null;
                return View();
            }

            var current = await _context.Users
                .Include(u => u.UserSongs)
                .Include(u => u.Subscriptions)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            ViewBag.Current = current;

            var userSongIds = current?.UserSongs.Select(us => us.SongId).ToList();
            var songs = await _context.SongsDetails
                .Where(s => !userSongIds!.Contains(s.SongId))
                .ToListAsync();

            ViewBag.AllSongs = songs;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddSong(string artist, string title, string genre, int price, int userID, IFormFile uploadSong, IFormFile uploadImage)
        {
            if (uploadSong == null || uploadImage == null)
            {
                ViewBag.Message = "Lütfen bir şarkı dosyası yükleyin.";
                return View();
            }
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.UserId == userID);

            var songUrl = await SaveFileAsync(uploadSong, "songs");
            var imageUrl = await SaveFileAsync(uploadImage, "songImages");

            var song = new Song
            {
                Title = title,
                Price = price,
                Genre = genre,
                UploadDate = DateTime.Now,
                ClickTimes = 0,
                FilePath = songUrl,
                ImagePath = imageUrl
            };

            _context.Songs.Add(song);

            var role = _context.Roles.FirstOrDefault(r => r.RoleId == 3);
            if (role != null && !user.Roles.Any(r => r.RoleId == 3))
            {
                user.Roles.Add(role);
            }

            if (user != null)
                song.Artists.Add(user);

            await _context.SaveChangesAsync();
            ViewBag.Message = "Şarkı başarıyla eklendi.";

            if (!string.IsNullOrWhiteSpace(artist))
            {
                var names = artist.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0);

                foreach (var name in names)
                {
                    var requested = await _context.Users.FirstOrDefaultAsync(u => u.UserName == name);
                    if (requested != null)
                    {
                        await SendRequest(user, requested, song);
                    }
                }
            }

            return RedirectToAction("MySongList");
        }

        public async Task<IActionResult> DeleteSongAsync(int id)
        {
            var song = await _context.Songs
               .Include(s => s.Artists)
               .Include(s => s.UserSongs)
               .Include(s => s.PlaylistSongs)
               .Include(s => s.SongSingerRequests)
               .FirstOrDefaultAsync(s => s.SongId == id);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (song == null)
            {
                ViewBag.Message = "Şarkı bulunamadı.";
                return RedirectToAction("MySongList");
            }

            song.Artists.Remove(user);

            if (song.Artists.Count == 0)
            {
                _context.UserSongs.RemoveRange(song.UserSongs);
                _context.PlaylistSongs.RemoveRange(song.PlaylistSongs);
                _context.SongSingerRequests.RemoveRange(song.SongSingerRequests);
                DeletePhysicalFile(song.ImagePath);
                DeletePhysicalFile(song.FilePath);
                _context.Songs.Remove(song);
            }

            await _context.SaveChangesAsync();

            ViewBag.Message = "Şarkı başarıyla silindi.";
            return RedirectToAction("MySongList");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSong(SongFormModel model)
        {
            var existing = await _context.Songs
                .Include(s => s.Artists)
                .FirstOrDefaultAsync(s => s.SongId == model.SongId);
            if (existing == null) return NotFound();

            existing.Title = model.Title!;
            existing.Genre = model.Genre!;
            existing.Price = model.Price;

            if (model.UploadImage != null)
            {
                DeletePhysicalFile(existing.ImagePath);
                existing.ImagePath = await SaveFileAsync(model.UploadImage, "songImages");
            }

            if (!model.SongId.HasValue && model.UploadSong != null)
            {
                DeletePhysicalFile(existing.FilePath);
                existing.ClickTimes = 0;
                existing.FilePath = await SaveFileAsync(model.UploadSong, "songs");
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(model.Artist))
            {
                var names = model.Artist.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0);

                var sender = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);
                foreach (var name in names)
                {
                    var requested = await _context.Users.FirstOrDefaultAsync(u => u.UserName == name);
                    if (requested != null && sender != null)
                    {
                        await SendRequest(sender, requested, existing);
                    }
                }
            }

            return RedirectToAction("MySongList");
        }

        public async Task<IActionResult> MySongListAsync()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);
            var songs = await _context.Songs.Where(s => s.Artists.Any(a => a.UserId == user!.UserId)).ToListAsync();

            var requests = await _context.SongSingerRequests.Where(u => u.RequestedUserId == user.UserId)
               .Select(r => new SongSingerRequestViewModel
               {
                   RequestId = r.Id,
                   SongTitle = r.Song.Title,
                   SenderUserName = r.Sender.UserName,
                   CreatedAt = r.CreatedAt,
               }
               ).ToListAsync();

            ViewBag.MySongList = songs;
            ViewBag.Requests = requests;
            return View();
        }

        public async Task<IActionResult> AcceptRequest(int id)
        {
            var request = await _context.SongSingerRequests.FirstOrDefaultAsync(r => r.Id == id);

            var user = await _context.Users.Where(u => u.UserId == request.RequestedUserId).FirstOrDefaultAsync();
            var song = await _context.Songs.Where(s => s.SongId == request.SongId).FirstOrDefaultAsync();
            if (user != null)
            {
                user.Songs.Add(song);
                _context.SongSingerRequests.Remove(request);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("MySongList");
        }

        public async Task<IActionResult> RejectRequest(int id)
        {
            var request = await _context.SongSingerRequests.FirstOrDefaultAsync(r => r.Id == id);
            _context.SongSingerRequests.Remove(request);
            await _context.SaveChangesAsync();

            return RedirectToAction("MySongList");
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Search(string searchTerm)
        {
            var songs = _context.SongsDetails
                .Where(s => s.Title.Contains(searchTerm) ||
                            s.Singers!.Contains(searchTerm))
                .ToList();
            ViewBag.AllSongs = songs!;
            return View("Index");
        }

        [AllowAnonymous] 
        public async Task<IActionResult> SongDetailsAsync(int id)
        {
            var current = await _context.Users.Include(u => u.UserSongs).Include(u => u.Subscriptions).FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            ViewBag.Current = current;
            var song = await _context.SongsDetails.FirstOrDefaultAsync(s => s.SongId == id);
            ViewBag.Song = song;

            bool userOwnsSong = false;
            bool userIsArtist = false;

            if (current != null)
            {
                userOwnsSong = current.UserSongs.Any(us => us.SongId == id);

                var songArtists = await _context.Songs.Where(s => s.SongId == id).SelectMany(s => s.Artists).Select(a => a.UserId).ToListAsync();

                userIsArtist = songArtists.Contains(current.UserId);
            }

            ViewBag.UserOwnsSong = userOwnsSong;
            ViewBag.UserIsArtist = userIsArtist;

            return View();
        }

        [HttpGet]
        public IActionResult GetSongForm(int? id)
        {
            SongFormModel vm;
            if (id.HasValue)
            {
                var song = _context.Songs.Include(s => s.Artists)
                               .FirstOrDefault(s => s.SongId == id.Value);
                vm = new SongFormModel
                {
                    SongId = song.SongId,
                    Title = song.Title,
                    Genre = song.Genre,
                    Price = song.Price,
                };
            }
            else
            {
                vm = new SongFormModel();
            }
            return PartialView("_AddorUpdateSongForm", vm);
        }

        public async Task SendRequest(User sender, User requested, Song song)
        {
            var request = _context.SongSingerRequests.Add(new SongSingerRequest
            {
                SongId = song.SongId,
                RequestedUserId = requested.UserId,
                SenderId = sender.UserId,
                CreatedAt = DateTime.Now,
            });
            await _context.SaveChangesAsync();
        }

        private async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            var ext = Path.GetExtension(file.FileName);
            var uniqueName = $"{Guid.NewGuid()}{ext}";

            var folder = Path.Combine(_env.WebRootPath, folderName);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var physicalPath = Path.Combine(folder, uniqueName);
            await using var stream = new FileStream(physicalPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/{folderName}/{uniqueName}";
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

        [HttpPost]
        public async Task<IActionResult> TrackPlay(int songId)
        {
            try
            {
                var song = await _context.Songs.FirstOrDefaultAsync(s => s.SongId == songId);
                if (song == null) return NotFound();

                var user = await _context.Users
                    .Include(u => u.UserSongs)
                    .Include(u => u.Subscriptions)
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (user != null)
                {
                    await _currentUserService.EnsureLoadedAsync();
                    var hasActiveSubscription = _currentUserService.HasActiveSubscription();

                    var ownsSong = user.UserSongs.Any(us => us.SongId == songId);

                    if (hasActiveSubscription && !ownsSong)
                    {
                        song.ClickTimes += 1; 
                        await _context.SaveChangesAsync();

                        return Json(new { success = true, clickTimes = song.ClickTimes });
                    }
                }

                return Json(new { success = false, message = "Play not tracked" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}