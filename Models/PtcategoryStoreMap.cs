using System;
using System.Collections.Generic;

namespace PrepTimerAPIs.Models;

public partial class PtcategoryStoreMap
{
    public int CategoryStoreMapId { get; set; }

    public int CategoryId { get; set; }

    public int StoreId { get; set; }

    public virtual Ptcategory Category { get; set; } = null!;

    public virtual Ptstore Store { get; set; } = null!;
}
