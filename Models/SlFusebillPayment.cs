using System;
using System.Collections.Generic;

namespace PrepTimerAPIs.Models;

public partial class SlFusebillPayment
{
    public int Id { get; set; }

    public string? EventType { get; set; }

    public string? EventSource { get; set; }

    public int? FusebillId { get; set; }

    public int? PaymentActivityId { get; set; }

    public string? Result { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? Text { get; set; }

    public double? Amount { get; set; }

    public double? UnAllocatedAmount { get; set; }

    public double? RefundableAmount { get; set; }

    public int? TransactionId { get; set; }

    public string? InvoiceAllocations { get; set; }
}
