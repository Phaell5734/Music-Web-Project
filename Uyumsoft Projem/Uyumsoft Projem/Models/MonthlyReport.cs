using System;
using System.Collections.Generic;

namespace Uyumsoft_Projem.Models;

public partial class MonthlyReport
{
    public int ReportId { get; set; }

    public int? ArtistId { get; set; }

    public string? ReportMonth { get; set; }

    public int? TotalPlays { get; set; }

    public decimal? TotalRevenue { get; set; }

    public decimal? PlatformCommission { get; set; }

    public decimal? ArtistPayout { get; set; }

    public virtual User? Artist { get; set; }
}
