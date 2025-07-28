using System;
using System.Collections.Generic;

namespace PrepTimerAPIs.Models;

public partial class SlFusebillProductMapping
{
    public int Id { get; set; }

    public string? FranchiseCode { get; set; }

    public int? FusebillProductId { get; set; }

    public string? FusebillProductName { get; set; }

    public int? SubscriptionId { get; set; }

    public string? SubscriptionType { get; set; }

    public int? EquipmentTypeId { get; set; }
}
