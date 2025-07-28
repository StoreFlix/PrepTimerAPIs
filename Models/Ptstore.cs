using System;
using System.Collections.Generic;

namespace PrepTimerAPIs.Models;

public partial class Ptstore
{
    public int StoreId { get; set; }

    public int? CompanyId { get; set; }

    public string StoreName { get; set; } = null!;

    public string Address1 { get; set; } = null!;

    public string? Address2 { get; set; }

    public string City { get; set; } = null!;

    public string State { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string Zipcode { get; set; } = null!;

    public string? Phonenumber { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual Ptcompany? Company { get; set; }

    public virtual ICollection<PtcategoryStoreMap> PtcategoryStoreMaps { get; set; } = new List<PtcategoryStoreMap>();
}
