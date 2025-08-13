using System;
using System.Collections.Generic;

namespace Uyumsoft_Projem.Models;

public partial class Subscription
{
    public int SubscriptionId { get; set; }

    public int? UserId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal? MonthlyFee { get; set; }

    public string? Status { get; set; }

    public virtual User? User { get; set; }
}
