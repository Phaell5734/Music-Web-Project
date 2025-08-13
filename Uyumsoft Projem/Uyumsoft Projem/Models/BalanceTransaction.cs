using System;
using System.Collections.Generic;

namespace Uyumsoft_Projem.Models;

public partial class BalanceTransaction
{
    public int TransactionId { get; set; }

    public int? UserId { get; set; }

    public decimal? Amount { get; set; }

    public string? Type { get; set; }

    public string? Description { get; set; }

    public DateTime? TransactionDate { get; set; }

    public virtual User? User { get; set; }
}
