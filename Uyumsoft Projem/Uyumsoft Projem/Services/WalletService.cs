using Microsoft.EntityFrameworkCore;
using Uyumsoft_Projem.Models;

namespace Uyumsoft_Projem.Services
{
    public class BalanceService
    {
        private readonly MusicPlatformContext _context;
        public BalanceService(MusicPlatformContext context)
        {
            _context = context;
        }

        public Task<decimal> BalanceAsync(int userId)
        {
            return _context.Users.Where(u => u.UserId == userId).Select(u => u.Balance ?? 0m).SingleAsync();
        }

        public async Task CreditAsync(int userId, decimal amount, string desc = "Yükleme")
        {
            var user = await _context.Users.FirstOrDefaultAsync(u=> u.UserId == userId);
            if (user == null) throw new InvalidOperationException("User not found");

            user.Balance = (user.Balance ?? 0m) + amount;

            _context.BalanceTransactions.Add(new BalanceTransaction
            {
                UserId = userId,
                Amount = amount,
                Type = "Load",
                Description = desc,
                TransactionDate = DateTime.Now
            });
            await _context.SaveChangesAsync();
        }

        public async Task<bool> TryDebitAsync(int userId, decimal amount, string desc = "Satın alma")
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || (user.Balance ?? 0m) < amount)
                return false;

            user.Balance -= amount;

            _context.BalanceTransactions.Add(new BalanceTransaction
            {
                UserId = userId,
                Amount = -amount,
                Type = "Purchase",
                Description = desc,
                TransactionDate = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return true;
        }
    }


}
