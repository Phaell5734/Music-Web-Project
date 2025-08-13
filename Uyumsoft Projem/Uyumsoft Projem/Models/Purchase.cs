using System;
using System.Collections.Generic;

namespace Uyumsoft_Projem.Models;

public partial class Purchase
{
    public int PurchaseId { get; set; }

    public int? UserId { get; set; }

    public string? OrderNumber { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public string? ReceiverAddress { get; set; }

    public virtual User? User { get; set; }
}
