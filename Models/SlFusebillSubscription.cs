using System;
using System.Collections.Generic;

namespace PrepTimerAPIs.Models;

public partial class SlFusebillSubscription
{
    public int Id { get; set; }

    public string? EventType { get; set; }

    public string? EventSource { get; set; }

    public int? FusebillId { get; set; }

    public int? SubscriptionId { get; set; }

    public string? Status { get; set; }

    public double? Amount { get; set; }

    public double? NetMrr { get; set; }

    public DateTime? SubscriptionDate { get; set; }
}
