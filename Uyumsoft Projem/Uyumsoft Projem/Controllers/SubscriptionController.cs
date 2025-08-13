using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Uyumsoft_Projem.Models;

namespace Uyumsoft_Projem.Controllers
{
    [Authorize]
    public class SubscriptionController : Controller
    {
        private readonly MusicPlatformContext _context;

        public SubscriptionController(MusicPlatformContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe()
        {
            if (User.Identity?.Name == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .Include(u => u.Roles)
                .Include(u => u.Subscriptions)  
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            
            if (user == null)
            {
                return NotFound();
            }

            var now = DateTime.Now;
            var activeSubscription = user.Subscriptions
                .FirstOrDefault(s => s.Status == "Active" && s.EndDate >= now);
                
            if (activeSubscription != null)
            {
                TempData["Message"] = "Zaten aktif bir Premium aboneliðiniz bulunmaktadýr.";
                return RedirectToAction("Index");
            }

            var isPremium = user.Roles.Any(r => r.RoleName == "premium");
            if (!isPremium)
            {
                var premiumRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "premium");
                if (premiumRole == null)
                {
                    premiumRole = new Role { RoleName = "premium" };
                    _context.Roles.Add(premiumRole);
                    await _context.SaveChangesAsync();
                }

                user.Roles.Add(premiumRole);
            }
            
            var subscription = new Subscription
            {
                UserId = user.UserId,
                StartDate = now,  
                EndDate = now.AddMonths(1),
                Status = "Active",
                MonthlyFee = 5
            };
            
            _context.Subscriptions.Add(subscription);
            
            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Premium aboneliðiniz baþarýyla aktifleþtirildi!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Abonelik iþlemi sýrasýnda bir hata oluþtu: " + ex.Message;
                return RedirectToAction("Index");
            }
            
            return RedirectToAction("Index", "Home");
        }
    }
}