using System;
using System.Collections.Generic;

namespace Uyumsoft_Projem.Models;

public partial class Song
{
    public int SongId { get; set; }

    public string Title { get; set; } = null!;

    public decimal Price { get; set; }

    public DateTime? UploadDate { get; set; }

    public int? ClickTimes { get; set; }

    public string FilePath { get; set; } = null!;

    public string ImagePath { get; set; } = null!;

    public string Genre { get; set; } = null!;

    public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();

    public virtual ICollection<SongSingerRequest> SongSingerRequests { get; set; } = new List<SongSingerRequest>();

    public virtual ICollection<UserSong> UserSongs { get; set; } = new List<UserSong>();

    public virtual ICollection<User> Artists { get; set; } = new List<User>();
}
