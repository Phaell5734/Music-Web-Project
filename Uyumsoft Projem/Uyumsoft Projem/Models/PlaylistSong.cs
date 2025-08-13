using System;
using System.Collections.Generic;

namespace Uyumsoft_Projem.Models;

public partial class PlaylistSong
{
    public int Id { get; set; }

    public int PlaylistId { get; set; }

    public int SongId { get; set; }

    public int OrderIndex { get; set; }

    public virtual Playlist Playlist { get; set; } = null!;

    public virtual Song Song { get; set; } = null!;
}
