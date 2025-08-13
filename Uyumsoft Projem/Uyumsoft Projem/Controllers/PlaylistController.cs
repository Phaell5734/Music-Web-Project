using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uyumsoft_Projem.Models;
using Uyumsoft_Projem.Services;
using Uyumsoft_Projem.ViewModels;

namespace Uyumsoft_Projem.Controllers
{
    [Authorize]
    public class PlaylistController : Controller
    {
        private readonly MusicPlatformContext _context;
        private readonly ICurrentUserService _currentUserService;

        public PlaylistController(MusicPlatformContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.Name == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var username = User.Identity.Name;
            var playlists = await _context.Playlists
                .Where(p => p.CreatorUsername == username)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(playlists);
        }

        public async Task<IActionResult> Details(int id)
        {
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.PlaylistId == id);

            if (playlist == null)
            {
                return NotFound();
            }

            var playlistSongs = await _context.PlaylistSongs
                .Where(ps => ps.PlaylistId == id)
                .OrderBy(ps => ps.OrderIndex)
                .Include(ps => ps.Song)
                .ToListAsync();

            await _currentUserService.EnsureLoadedAsync();
            var user = _currentUserService.Current;
            
            var songIds = user?.UserSongs.Select(us => us.SongId).ToList() ?? new List<int>();
            
            var viewModel = new PlaylistDetailViewModel
            {
                Playlist = playlist,
                Songs = playlistSongs.Select(ps => ps.Song).ToList(),
                OwnedSongIds = songIds,
                TotalPriceOfMissingSongs = playlistSongs
                    .Where(ps => !songIds.Contains(ps.SongId))
                    .Sum(ps => ps.Song.Price),
                HasActiveSubscription = _currentUserService.HasActiveSubscription() 
            };

            ViewBag.CurrentUser = user;
            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string title)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    return Json(new { success = false, message = "Playlist adý boþ olamaz" });
                }

                if (User.Identity?.Name == null)
                {
                    return Json(new { success = false, message = "You must be logged in." });
                }

                var playlist = new Playlist
                {
                    Title = title,
                    CreatorUsername = User.Identity.Name!,
                    CreatedAt = DateTime.Now
                };
                
                _context.Playlists.Add(playlist);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, playlistId = playlist.PlaylistId, message = "Playlist created successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddSongToPlaylist(int songId, List<int> playlistIds)
        {
            if (User.Identity?.Name == null)
            {
                return Json(new { success = false, message = "You must be logged in." });
            }

            var existingPlaylistSongs = await _context.PlaylistSongs
                .Where(ps => playlistIds.Contains(ps.PlaylistId))
                .GroupBy(ps => ps.PlaylistId)
                .ToDictionaryAsync(g => g.Key, g => g.Max(ps => ps.OrderIndex) + 1);
            
            foreach (var playlistId in playlistIds)
            {
                var exists = await _context.PlaylistSongs
                    .AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
                
                if (!exists)
                {
                    var orderIndex = existingPlaylistSongs.ContainsKey(playlistId) 
                        ? existingPlaylistSongs[playlistId] 
                        : 0;
                    
                    _context.PlaylistSongs.Add(new PlaylistSong
                    {
                        PlaylistId = playlistId,
                        SongId = songId,
                        OrderIndex = orderIndex
                    });
                }
            }
            
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSongFromPlaylist(int songId, int playlistId)
        {
            var playlistSong = await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
            
            if (playlistSong != null)
            {
                _context.PlaylistSongs.Remove(playlistSong);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            
            return Json(new { success = false, message = "Song not found in playlist." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSongOrder(int playlistId, List<int> songIds)
        {
            var playlistSongs = await _context.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .ToListAsync();
            
            for (int i = 0; i < songIds.Count; i++)
            {
                var song = playlistSongs.FirstOrDefault(ps => ps.SongId == songIds[i]);
                if (song != null)
                {
                    song.OrderIndex = i;
                }
            }
            
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserPlaylists()
        {
            try
            {
                if (User.Identity?.Name == null)
                {
                    return Json(new { success = false, message = "User not logged in", playlists = new List<object>() });
                }

                var playlists = await _context.Playlists
                    .Where(p => p.CreatorUsername == User.Identity.Name!)
                    .Select(p => new { p.PlaylistId, p.Title })
                    .ToListAsync();
                
                return Json(new { success = true, playlists });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, playlists = new List<object>() });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAndAddSong(string title, int songId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    return Json(new { success = false, message = "Playlist adý boþ olamaz" });
                }

                if (User.Identity?.Name == null)
                {
                    return Json(new { success = false, message = "You must be logged in." });
                }

                var playlist = new Playlist
                {
                    Title = title,
                    CreatorUsername = User.Identity.Name!,
                    CreatedAt = DateTime.Now
                };
                
                _context.Playlists.Add(playlist);
                await _context.SaveChangesAsync();

                var playlistSong = new PlaylistSong
                {
                    PlaylistId = playlist.PlaylistId,
                    SongId = songId,
                    OrderIndex = 0
                };

                _context.PlaylistSongs.Add(playlistSong);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, playlistId = playlist.PlaylistId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPlaylistSongs(int playlistId)
        {
            try
            {
                var playlistSongs = await _context.PlaylistSongs
                    .Where(ps => ps.PlaylistId == playlistId)
                    .OrderBy(ps => ps.OrderIndex)
                    .Include(ps => ps.Song)
                    .Select(ps => new { 
                        songId = ps.Song.SongId, 
                        title = ps.Song.Title, 
                        filePath = ps.Song.FilePath, 
                        imagePath = ps.Song.ImagePath 
                    })
                    .ToListAsync();
                    
                return Json(new { success = true, songs = playlistSongs });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, songs = new List<object>() });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SearchInPlaylist(int playlistId, string searchTerm)
        {
            try
            {
                var playlist = await _context.Playlists
                    .FirstOrDefaultAsync(p => p.PlaylistId == playlistId);

                if (playlist == null)
                {
                    return NotFound();
                }

                var playlistSongs = await _context.PlaylistSongs
                    .Where(ps => ps.PlaylistId == playlistId)
                    .OrderBy(ps => ps.OrderIndex)
                    .Include(ps => ps.Song)
                    .ThenInclude(s => s.Artists)
                    .ToListAsync();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    playlistSongs = playlistSongs.Where(ps => 
                        ps.Song.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        ps.Song.Genre.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        ps.Song.Artists.Any(a => a.UserName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                await _currentUserService.EnsureLoadedAsync();
                var user = _currentUserService.Current;
                var songIds = user?.UserSongs.Select(us => us.SongId).ToList() ?? new List<int>();
                
                var viewModel = new PlaylistDetailViewModel
                {
                    Playlist = playlist,
                    Songs = playlistSongs.Select(ps => ps.Song).ToList(),
                    OwnedSongIds = songIds,
                    TotalPriceOfMissingSongs = playlistSongs
                        .Where(ps => !songIds.Contains(ps.SongId))
                        .Sum(ps => ps.Song.Price),
                    HasActiveSubscription = _currentUserService.HasActiveSubscription() 
                };

                ViewBag.CurrentUser = user;
                
                return View("Details", viewModel);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}