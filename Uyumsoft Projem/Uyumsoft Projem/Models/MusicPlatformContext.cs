using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Uyumsoft_Projem.Models;

public partial class MusicPlatformContext : DbContext
{
    public MusicPlatformContext()
    {
    }

    public MusicPlatformContext(DbContextOptions<MusicPlatformContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BalanceTransaction> BalanceTransactions { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<MonthlyReport> MonthlyReports { get; set; }

    public virtual DbSet<Playlist> Playlists { get; set; }

    public virtual DbSet<PlaylistSong> PlaylistSongs { get; set; }

    public virtual DbSet<Purchase> Purchases { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Song> Songs { get; set; }

    public virtual DbSet<SongSingerRequest> SongSingerRequests { get; set; }

    public virtual DbSet<SongsDetail> SongsDetails { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserSong> UserSongs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=MusicPlatform;Integrated Security=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BalanceTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__BalanceT__55433A4B7FA8E06B");

            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.TransactionDate).HasColumnType("datetime");
            entity.Property(e => e.Type).HasMaxLength(255);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.BalanceTransactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__BalanceTr__UserI__797309D9");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoices__D796AAD5013EDAB4");

            entity.HasIndex(e => e.InvoiceNumber, "UQ__Invoices__D776E981F785887F").IsUnique();

            entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNumber).HasMaxLength(255);
            entity.Property(e => e.TargetId).HasColumnName("TargetID");
            entity.Property(e => e.TargetType).HasMaxLength(255);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Invoices__UserID__75A278F5");
        });

        modelBuilder.Entity<MonthlyReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__MonthlyR__D5BD48E51C064DA3");

            entity.Property(e => e.ReportId).HasColumnName("ReportID");
            entity.Property(e => e.ArtistId).HasColumnName("ArtistID");
            entity.Property(e => e.ArtistPayout).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.PlatformCommission).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.ReportMonth).HasMaxLength(255);
            entity.Property(e => e.TotalRevenue).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.Artist).WithMany(p => p.MonthlyReports)
                .HasForeignKey(d => d.ArtistId)
                .HasConstraintName("FK__MonthlyRe__Artis__76969D2E");
        });

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasKey(e => e.PlaylistId).HasName("PK__Playlist__B30167A076C1AAD5");

            entity.ToTable("Playlist");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreatorUsername).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<PlaylistSong>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Playlist__3214EC078900563E");

            entity.ToTable("PlaylistSong");

            entity.HasIndex(e => new { e.PlaylistId, e.OrderIndex }, "UQ_PlaylistSong_Playlist_Order").IsUnique();

            entity.HasOne(d => d.Playlist).WithMany(p => p.PlaylistSongs)
                .HasForeignKey(d => d.PlaylistId)
                .HasConstraintName("FK_PlaylistSong_Playlist");

            entity.HasOne(d => d.Song).WithMany(p => p.PlaylistSongs)
                .HasForeignKey(d => d.SongId)
                .HasConstraintName("FK_PlaylistSong_Song");
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasKey(e => e.PurchaseId).HasName("PK__Purchase__6B0A6BDEB9A23B99");

            entity.HasIndex(e => e.OrderNumber, "UQ__Purchase__CAC5E743074053F6").IsUnique();

            entity.Property(e => e.PurchaseId).HasColumnName("PurchaseID");
            entity.Property(e => e.OrderNumber).HasMaxLength(255);
            entity.Property(e => e.PurchaseDate).HasColumnType("datetime");
            entity.Property(e => e.ReceiverAddress).HasMaxLength(255);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Purchases__UserI__74AE54BC");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3AF511B672");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(255);
        });

        modelBuilder.Entity<Song>(entity =>
        {
            entity.HasKey(e => e.SongId).HasName("PK__Songs__12E3D6F732FBCE8F");

            entity.Property(e => e.SongId).HasColumnName("SongID");
            entity.Property(e => e.FilePath)
                .HasMaxLength(255)
                .HasDefaultValue("");
            entity.Property(e => e.Genre)
                .HasMaxLength(100)
                .HasDefaultValue("");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .HasDefaultValue("");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UploadDate).HasColumnType("datetime");

            entity.HasMany(d => d.Artists).WithMany(p => p.Songs)
                .UsingEntity<Dictionary<string, object>>(
                    "SongArtist",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__SongArtis__Artis__40058253"),
                    l => l.HasOne<Song>().WithMany()
                        .HasForeignKey("SongId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__SongArtis__SongI__40F9A68C"),
                    j =>
                    {
                        j.HasKey("SongId", "ArtistId");
                        j.ToTable("SongArtists");
                        j.IndexerProperty<int>("SongId").HasColumnName("SongID");
                        j.IndexerProperty<int>("ArtistId").HasColumnName("ArtistID");
                    });
        });

        modelBuilder.Entity<SongSingerRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC073181D5E5");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.RequestedUser).WithMany(p => p.SongSingerRequestRequestedUsers)
                .HasForeignKey(d => d.RequestedUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SongSingerRequests_Requested");

            entity.HasOne(d => d.Sender).WithMany(p => p.SongSingerRequestSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SongSingerRequests_Senders");

            entity.HasOne(d => d.Song).WithMany(p => p.SongSingerRequests)
                .HasForeignKey(d => d.SongId)
                .HasConstraintName("FK_SongSingerRequests_Songs");
        });

        modelBuilder.Entity<SongsDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("SongsDetail");

            entity.Property(e => e.FilePath).HasMaxLength(255);
            entity.Property(e => e.Genre).HasMaxLength(100);
            entity.Property(e => e.ImagePath).HasMaxLength(255);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Singers).HasMaxLength(4000);
            entity.Property(e => e.SongId).HasColumnName("SongID");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UploadDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId).HasName("PK__Subscrip__9A2B24BD7DDD010E");

            entity.Property(e => e.SubscriptionId).HasColumnName("SubscriptionID");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.MonthlyFee).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(255);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Subscript__UserI__70DDC3D8");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC4E9A45E8");

            entity.HasIndex(e => e.UserName, "UQ__Users__C9F284565904FAB6").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Balance).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(255);
            entity.Property(e => e.LastName).HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.PasswordSalt).HasMaxLength(128);
            entity.Property(e => e.PhoneNumber).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasDefaultValue("Active");
            entity.Property(e => e.Tcno)
                .HasMaxLength(255)
                .HasColumnName("TCNo");
            entity.Property(e => e.UserName).HasMaxLength(255);

            entity.HasMany(d => d.Artists).WithMany(p => p.Followers)
                .UsingEntity<Dictionary<string, object>>(
                    "Follow",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Follows__ArtistI__5224328E"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("FollowerId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Follows__Followe__531856C7"),
                    j =>
                    {
                        j.HasKey("FollowerId", "ArtistId");
                        j.ToTable("Follows");
                        j.IndexerProperty<int>("FollowerId").HasColumnName("FollowerID");
                        j.IndexerProperty<int>("ArtistId").HasColumnName("ArtistID");
                    });

            entity.HasMany(d => d.Followers).WithMany(p => p.Artists)
                .UsingEntity<Dictionary<string, object>>(
                    "Follow",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("FollowerId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Follows__Followe__531856C7"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Follows__ArtistI__5224328E"),
                    j =>
                    {
                        j.HasKey("FollowerId", "ArtistId");
                        j.ToTable("Follows");
                        j.IndexerProperty<int>("FollowerId").HasColumnName("FollowerID");
                        j.IndexerProperty<int>("ArtistId").HasColumnName("ArtistID");
                    });

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserRoles_Role"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserRoles_User"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("UserRoles");
                        j.IndexerProperty<int>("UserId").HasColumnName("UserID");
                        j.IndexerProperty<int>("RoleId").HasColumnName("RoleID");
                    });
        });

        modelBuilder.Entity<UserSong>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.SongId });

            entity.ToTable("UserSong");

            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Song).WithMany(p => p.UserSongs)
                .HasForeignKey(d => d.SongId)
                .HasConstraintName("FK_UserSong_Songs");

            entity.HasOne(d => d.User).WithMany(p => p.UserSongs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserSong_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
