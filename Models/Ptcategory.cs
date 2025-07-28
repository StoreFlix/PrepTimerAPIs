using System;
using System.Collections.Generic;

namespace PrepTimerAPIs.Models;

public partial class Ptcategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public int? CompanyId { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Ptcompany? Company { get; set; }

    public virtual ICollection<PtcategoryStoreMap> PtcategoryStoreMaps { get; set; } = new List<PtcategoryStoreMap>();
}
