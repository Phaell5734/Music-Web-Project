using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Uyumsoft_Projem.Models;
using Uyumsoft_Projem.Services;

namespace Uyumsoft_Projem.Controllers
{
    [Authorize]
    public class WalletController : Controller
    {
        private readonly MusicPlatformContext _context;
        private readonly BalanceService _bal;

        public WalletController(BalanceService bal, MusicPlatformContext context)
        {
            _bal = bal;
            _context = context;
        }

        public async Task<IActionResult> Balance()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            ViewBag.Balance = await _bal.BalanceAsync(user.UserId);
            return View();                  
        }

        [HttpGet]
        public async Task<IActionResult> Current()          
        {
            var user = await _context.Users
                         .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            if (user == null) return Json(new { ok = false, bal = 0 });

            var bal = await _bal.BalanceAsync(user.UserId);
            return Json(new { ok = true, bal });
        }
        
        [HttpPost]
        public async Task<IActionResult> TopUp(decimal amount)
        {
            if (amount <= 0) return BadRequest("Miktar > 0 olmalı");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            await _bal.CreditAsync(user.UserId, amount, "Kredi kartı ile yükleme");
            return RedirectToAction(nameof(Balance));
        }
    }
}
