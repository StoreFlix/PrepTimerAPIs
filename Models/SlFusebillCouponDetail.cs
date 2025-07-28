using System;
using System.Collections.Generic;

namespace PrepTimerAPIs.Models;

public partial class SlFusebillCouponDetail
{
    public int Id { get; set; }

    public int? FusebillId { get; set; }

    public int? PlanId { get; set; }

    public string? CouponCode { get; set; }

    public int? CouponId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public DateTime? EligibilityStartDate { get; set; }

    public DateTime? EligibilityEndDate { get; set; }

    public string? Status { get; set; }

    public int? PlanProductUniqueId { get; set; }

    public DateTime? CouponDetailDate { get; set; }
}
