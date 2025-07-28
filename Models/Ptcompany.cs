using System;
using System.Collections.Generic;

namespace PrepTimerAPIs.Models;

public partial class Ptcompany
{
    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string Address1 { get; set; } = null!;

    public string? Address2 { get; set; }

    public string City { get; set; } = null!;

    public string State { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string Zipcode { get; set; } = null!;

    public string? Phonenumber { get; set; }

    public int? FusebillId { get; set; }

    public string? FranchiseCode { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual ICollection<PtactiveDevice> PtactiveDevices { get; set; } = new List<PtactiveDevice>();

    public virtual ICollection<Ptcategory> Ptcategories { get; set; } = new List<Ptcategory>();

    public virtual ICollection<PtcustomerSubscription> PtcustomerSubscriptions { get; set; } = new List<PtcustomerSubscription>();

    public virtual ICollection<Ptstore> Ptstores { get; set; } = new List<Ptstore>();

    public virtual ICollection<PTUser> PTUsers { get; set; } = new List<PTUser>();
}
