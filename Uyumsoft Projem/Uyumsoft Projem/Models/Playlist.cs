using System;
using System.Collections.Generic;

namespace Uyumsoft_Projem.Models;

public partial class Playlist
{
    public int PlaylistId { get; set; }

    public string Title { get; set; } = null!;

    public string CreatorUsername { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
}
