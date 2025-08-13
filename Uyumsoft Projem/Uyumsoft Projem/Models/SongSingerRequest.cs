using System;
using System.Collections.Generic;

namespace Uyumsoft_Projem.Models;

public partial class SongSingerRequest
{
    public int Id { get; set; }

    public int? SongId { get; set; }

    public int RequestedUserId { get; set; }

    public int SenderId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User RequestedUser { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;

    public virtual Song? Song { get; set; }
}
