using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Uyumsoft_Projem.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Uyumsoft_Proje.Controllers
{
    public class AccountController : Controller
    {
        MusicPlatformContext _context = new MusicPlatformContext();

        //GET: Account - Login sayfası herkese açık
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.UserName == userName);
            if (existingUser != null)
            {
                // Check if user status is Active
                if (existingUser.Status != "Active")
                {
                    ViewBag.ErrorMessage = "Bu kullanıcı yasaklıdır. Giriş yapamazsınız.";
                    return View();
                }

                if (VerifyPassword(password, existingUser.PasswordHash, existingUser.PasswordSalt))
                {
                    await AuthenticateUserAsync(existingUser);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    Console.WriteLine("Şifreler eşleşmiyor.");
                }
            }
            else
            {
                Console.WriteLine("Kullanıcı adıyla kullanıcı bulunmamaktadır.");
            }
            ViewBag.ErrorMessage = "Kullanıcı adı veya şifre yanlış.";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string userName, string email, string password)
        {
            if (userName.Contains(',') || userName.Contains(' '))
                return Json(new { success = false, message = "Kullanıcı adında virgül veya boşluk kullanılamaz." });
            if (_context.Users.Any(u => u.UserName == userName))
            {
                return Json(new { success = false, message = "Bu kullanıcı adı alınmış." });
            }
            var hashResult = CreateHash(password);
            var user = new User()
            {
                UserName = userName,
                Email = email,
                PasswordHash = hashResult.Hash,
                PasswordSalt = hashResult.Salt,
                Status = "Active" // Set default status to Active
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            await AuthenticateUserAsync(user);
            return Json(new { success = true, message = "Kayıt başarılı." });
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View("Login");
        }

        private async Task AuthenticateUserAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ViewBag.user = user;
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
        }

        //Hash Kodu internetten alınmıştır.
        public static (byte[] Hash, byte[] Salt) CreateHash(string password, int iterations = 100_000)
        {
            // 128‑bit (16 byte) salt
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            // PBKDF2 ile 256‑bit (32 byte) hash üret
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            return (hash, salt);
        }

        // Girilen şifreyi storedHash ve storedSalt ile karşılaştırır
        public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt, int iterations = 100_000)
        {
            // Aynı parametrelerle hash'i yeniden üret
            using var pbkdf2 = new Rfc2898DeriveBytes(password, storedSalt, iterations, HashAlgorithmName.SHA256);
            byte[] computedHash = pbkdf2.GetBytes(storedHash.Length);

            // Sabit zamanlı karşılaştırma
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }

        [Authorize] 
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null) return RedirectToAction("Login");

            return View(user);
        }

        [Authorize] 
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string firstName, string lastName, string address, string tcno, string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null) return Json(new { success = false, message = "Kullanıcı bulunamadı." });

            user.FirstName = firstName;
            user.LastName = lastName;
            user.Address = address;
            user.Tcno = tcno;
            user.Email = email;

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Profil bilgileri güncellendi." });
        }

        [Authorize] 
        [HttpGet]
        public async Task<IActionResult> CheckProfileComplete()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null) return Json(new { complete = false });

            bool isComplete = !string.IsNullOrWhiteSpace(user.FirstName) &&
                             !string.IsNullOrWhiteSpace(user.LastName) &&
                             !string.IsNullOrWhiteSpace(user.Address) &&
                             !string.IsNullOrWhiteSpace(user.Tcno) &&
                             !string.IsNullOrWhiteSpace(user.Email);

            return Json(new { complete = isComplete, hasBalance = user.Balance > 0 });
        }
    }
}