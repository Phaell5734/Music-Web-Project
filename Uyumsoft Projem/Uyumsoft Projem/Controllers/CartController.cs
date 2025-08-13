using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Uyumsoft_Projem.Models;
using Uyumsoft_Projem.Services;

namespace Uyumsoft_Projem.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cart;
        private readonly BalanceService _bal;
        private readonly MusicPlatformContext _context;

        public CartController(ICartService cart, BalanceService bal, MusicPlatformContext ctx)
        {
            _cart = cart;
            _bal = bal;
            _context = ctx;
        }

        [HttpPost]       
        public IActionResult Add(int id)
        {
            var song = _context.Songs.FirstOrDefault(s => s.SongId == id);
            if (song == null) return NotFound();
            _cart.Add(new CartItem(song.SongId, song.Title, song.Price));
            return Json(new { count = _cart.GetCart().Count() });
        }

        public IActionResult ViewCart()
        {
            return View(_cart.GetCart());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var items = _cart.GetCart().ToList();
            if (!items.Any()) return BadRequest("Sepet boş");

            var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity!.Name);
    
            bool hasImportantInfo = !string.IsNullOrWhiteSpace(user.Address) && 
                                   !string.IsNullOrWhiteSpace(user.Tcno);
    
            if (!hasImportantInfo)
            {
                return Json(new { 
                    success = false, 
                    message = "Lütfen satın alma işlemi yapmadan önce adres ve TC kimlik bilgilerinizi doldurun.",
                    redirectToProfile = true 
                });
            }

            decimal total = items.Sum(i => i.Price);

            if (!await _bal.TryDebitAsync(user!.UserId, total, "Sepet satın alımı"))
                return BadRequest("Bakiye yetersiz");

            foreach (var it in items)
            {
                bool exists = _context.UserSongs.Any(us => us.UserId == user.UserId && us.SongId == it.SongId);
                if (!exists)
                {
                    _context.UserSongs.Add(new UserSong
                    {
                        UserId = user.UserId,
                        SongId = it.SongId,
                        AddedAt = DateTime.UtcNow
                    });
                }
            }

            var purchase = new Purchase
            {
                UserId = user.UserId,
                OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}",   
                PurchaseDate = DateTime.UtcNow,
                ReceiverAddress = user.Address          
            };
            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            var invoice = _context.Invoices.Add(new Invoice
            {
                UserId = user.UserId,
                InvoiceNumber = $"INV-{purchase.OrderNumber}",
                TargetType = "Purchase",
                TargetId = purchase.PurchaseId,
                Amount = total,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            await DistributeSalesCommissionAsync(items, total);

            _cart.Clear();
            return Json(new { success = true, message = "Purchase completed successfully!" });
        }

        private async Task DistributeSalesCommissionAsync(List<CartItem> items, decimal total)
        {
            var admin = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Roles.Any(r => r.RoleName == "admin"));

            if (admin == null) return;

            // 15% platform commission, 85% to artists
            var platformCommission = total * 0.15m;
            var artistShare = total * 0.85m;

            admin.Balance += platformCommission;
            _context.BalanceTransactions.Add(new BalanceTransaction
            {
                UserId = admin.UserId,
                Amount = platformCommission,
                Type = "Sales_Commission",
                Description = $"Sales commission from order totaling ${total}",
                TransactionDate = DateTime.Now
            });

            foreach (var item in items)
            {
                var song = await _context.Songs
                    .Include(s => s.Artists)
                    .FirstOrDefaultAsync(s => s.SongId == item.SongId);

                if (song?.Artists.Any() == true)
                {
                    var sharePerArtist = artistShare / items.Count / song.Artists.Count;
                    
                    foreach (var artist in song.Artists)
                    {
                        artist.Balance = (artist.Balance ?? 0) + sharePerArtist;
                        _context.BalanceTransactions.Add(new BalanceTransaction
                        {
                            UserId = artist.UserId,
                            Amount = sharePerArtist,
                            Type = "Sales_Revenue",
                            Description = $"Revenue from sale of '{song.Title}'",
                            TransactionDate = DateTime.Now
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        [HttpGet]
        public IActionResult Count()
        {
            return Json(new { count = _cart.GetCart().Count() });
        }

        public IActionResult Index()
        {
            return View(_cart.GetCart());
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            _cart.Remove(id);
            return Json(new { count = _cart.GetCart().Count() });
        }

        public IActionResult Success()
        {
            TempData["msg"] = "Satın alma başarılı! Şarkılar kütüphanenize eklendi.";
            return RedirectToAction(nameof(Index));
        }
    }

}
