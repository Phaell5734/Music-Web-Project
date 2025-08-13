using System;
using System.Collections.Generic;

namespace Uyumsoft_Projem.Models;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public byte[] PasswordHash { get; set; } = null!;

    public byte[] PasswordSalt { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Tcno { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public decimal? Balance { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<BalanceTransaction> BalanceTransactions { get; set; } = new List<BalanceTransaction>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<MonthlyReport> MonthlyReports { get; set; } = new List<MonthlyReport>();

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    public virtual ICollection<SongSingerRequest> SongSingerRequestRequestedUsers { get; set; } = new List<SongSingerRequest>();

    public virtual ICollection<SongSingerRequest> SongSingerRequestSenders { get; set; } = new List<SongSingerRequest>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual ICollection<UserSong> UserSongs { get; set; } = new List<UserSong>();

    public virtual ICollection<User> Artists { get; set; } = new List<User>();

    public virtual ICollection<User> Followers { get; set; } = new List<User>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
}
