using System;
using System.Collections.Generic;

namespace PrepTimerAPIs.Models;

public partial class PTUser
{
    public int UserId { get; set; }

    public int? CompanyId { get; set; }

    public string? LoginName { get; set; }

    public string Email { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Phonenumber { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public int? ModifiedBy { get; set; }

    public string? ResetPasswordToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }

    public virtual Ptcompany? Company { get; set; }

    public virtual ICollection<PtactiveDevice> PtactiveDevices { get; set; } = new List<PtactiveDevice>();

    public virtual ICollection<PtcustomerSubscription> PtcustomerSubscriptions { get; set; } = new List<PtcustomerSubscription>();
}
