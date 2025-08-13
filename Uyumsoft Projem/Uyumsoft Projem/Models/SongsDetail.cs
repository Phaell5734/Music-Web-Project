using System;
using System.Collections.Generic;

namespace Uyumsoft_Projem.Models;

public partial class SongsDetail
{
    public int SongId { get; set; }

    public string Title { get; set; } = null!;

    public decimal Price { get; set; }

    public string Genre { get; set; } = null!;

    public DateTime? UploadDate { get; set; }

    public int? ClickTimes { get; set; }

    public string ImagePath { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public string? Singers { get; set; }
}
