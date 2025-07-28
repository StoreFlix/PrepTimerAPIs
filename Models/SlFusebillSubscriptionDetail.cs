using System;
using System.Collections.Generic;

namespace PrepTimerAPIs.Models;

public partial class SlFusebillSubscriptionDetail
{
    public int Id { get; set; }

    public int? FusebillId { get; set; }

    public int? SubscriptionId { get; set; }

    public int? SubscriptionProductId { get; set; }

    public int? Quantity { get; set; }

    public int? Price { get; set; }

    public string? Currency { get; set; }

    public string? Status { get; set; }

    public int? PlanId { get; set; }

    public string? PlanName { get; set; }

    public int? PlanFrequencyId { get; set; }

    public int? ProductId { get; set; }

    public string? ProductName { get; set; }

    public int? PlanProductUniqueId { get; set; }

    public DateTime? NextRechargeDate { get; set; }

    public DateTime? LastPurchaseDate { get; set; }

    public DateTime? SubscriptionDetailDate { get; set; }
}
