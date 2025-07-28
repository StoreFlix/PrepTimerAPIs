using System;
using System.Collections.Generic;

namespace PrepTimerAPIs.Models;

public partial class Ptlanguage
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Locale { get; set; }

    public string? FilePath { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }
}
