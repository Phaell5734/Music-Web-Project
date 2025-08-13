using Uyumsoft_Projem.Models;

namespace Uyumsoft_Projem.Services
{
    public interface ICurrentUserService
    {
        User? Current { get; }            // null → anonymous
        Task EnsureLoadedAsync();          // lazy load
        bool HasActiveSubscription();
        bool IsAdmin();  // Bu satırı ekleyin
    }
}
