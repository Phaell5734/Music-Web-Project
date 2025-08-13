using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Uyumsoft_Projem.Models;
using Uyumsoft_Projem.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// ➜ Distributed cache ve Session
builder.Services.AddDistributedMemoryCache();          // RAM tabanlı
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".SolMusic.Session";
    options.IdleTimeout = TimeSpan.FromHours(4); // oturum yaşam süresi
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;                // GDPR zorunlu cookie
});

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
       .AddCookie(opts => opts.LoginPath = "/Account/Login");

// (CartService için ihtiyaç olacak)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICartService, SessionCartService>();
builder.Services.AddScoped<BalanceService>();
builder.Services.AddDbContext<MusicPlatformContext>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddHostedService<SubscriptionExpirationService>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session middleware mutlaka auth’tan ÖNCE/NORMALDE sonra
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
