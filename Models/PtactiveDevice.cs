using System;
using System.Collections.Generic;

namespace PrepTimerAPIs.Models;

public partial class PtactiveDevice
{
    public int DeviceId { get; set; }

    public int CompanyId { get; set; }

    public int? SubscriptionId { get; set; }

    public string? DeviceUniqueId { get; set; }

    public string? DeviceType { get; set; }

    public string? DeviceOs { get; set; }

    public string? DeviceImei { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public virtual Ptcompany Company { get; set; } = null!;

    public virtual PTUser? CreatedByNavigation { get; set; }

    public virtual PtcustomerSubscription? Subscription { get; set; }
}
