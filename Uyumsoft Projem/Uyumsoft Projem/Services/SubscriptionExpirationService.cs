using Microsoft.EntityFrameworkCore;
using Uyumsoft_Projem.Models;

namespace Uyumsoft_Projem.Services
{
    public class SubscriptionExpirationService : BackgroundService
    {
        private readonly IServiceProvider _services;
        
        public SubscriptionExpirationService(IServiceProvider services)
        {
            _services = services;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<MusicPlatformContext>();

                    var expiredSubscriptions = await context.Subscriptions.Where(s => s.Status == "Active" && s.EndDate < DateTime.Now).ToListAsync();
                    
                    foreach (var subscription in expiredSubscriptions)
                    {
                        subscription.Status = "Inactive";
                    }
                    
                    await context.SaveChangesAsync();
                }
                
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
