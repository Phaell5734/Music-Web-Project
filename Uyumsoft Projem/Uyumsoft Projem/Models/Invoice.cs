using System;
using System.Collections.Generic;

namespace Uyumsoft_Projem.Models;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public string? InvoiceNumber { get; set; }

    public string? TargetType { get; set; }

    public int? TargetId { get; set; }

    public int? UserId { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
