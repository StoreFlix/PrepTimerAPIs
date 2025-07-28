using System;
using System.Collections.Generic;

namespace PrepTimerAPIs.Models;

public partial class PtcustomerSubscription
{
    public int SubscriptionId { get; set; }

    public int CompanyId { get; set; }

    public int Subscriptions { get; set; }

    public DateTime ExpiryDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public int? FusebillId { get; set; }

    public int? FbsubscriptionId { get; set; }

    public bool? IsActive { get; set; }

    public virtual Ptcompany Company { get; set; } = null!;

    public virtual PTUser? CreatedByNavigation { get; set; }

    public virtual ICollection<PtactiveDevice> PtactiveDevices { get; set; } = new List<PtactiveDevice>();
}
