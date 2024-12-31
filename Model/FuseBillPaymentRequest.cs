using System;
using System.Collections.Generic;

namespace ServiceFabricApp.API.Model
{
    public class FuseBillPaymentRequest
    {
        /// <summary>
        /// EventType
        /// </summary>
        public string? EventType { get; set; }

        /// <summary>
        /// EventSource
        /// </summary>
        public string? EventSource { get; set; }

        /// <summary>
        /// Payment
        /// </summary>
        public Payment? Payment { get; set; }

        /// <summary>
        /// PaymentMethod
        /// </summary>
        public PaymentMethod? PaymentMethod { get; set; }

    }

    public class Payment
    {
        /// <summary>
        /// createdTimestamp
        /// </summary>
        public DateTime? createdTimestamp { get; set; }

        /// <summary>
        /// paymentSource
        /// </summary>
        public string? paymentSource { get; set; }

        /// <summary>
        /// paymentActivityId
        /// </summary>
        public int? paymentActivityId { get; set; }

        /// <summary>
        /// reference
        /// </summary>
        public string? reference { get; set; }

        /// <summary>
        /// effectiveTimestamp
        /// </summary>
        public DateTime? effectiveTimestamp { get; set; }

        /// <summary>
        /// description
        /// </summary>
        public string? description { get; set; }

        /// <summary>
        /// customerId
        /// </summary>
        public int? customerId { get; set; }

        /// <summary>
        /// originalPaymentActivityId
        /// </summary>
        public int? originalPaymentActivityId { get; set; }

        /// <summary>
        /// amount
        /// </summary>
        public double? amount { get; set; }

        /// <summary>
        /// currency
        /// </summary>
        public string? currency { get; set; }

        /// <summary>
        /// invoiceAllocations
        /// </summary>
        public List<InvoiceAllocation>? invoiceAllocations { get; set; }

        /// <summary>
        /// refunds
        /// </summary>
        public List<Refund>? refunds { get; set; }

        /// <summary>
        /// unallocatedAmount
        /// </summary>
        public double? unallocatedAmount { get; set; }

        /// <summary>
        /// refundableAmount
        /// </summary>
        public double? refundableAmount { get; set; }

        /// <summary>
        /// id
        /// </summary>
        public int? id { get; set; }

        /// <summary>
        /// uri
        /// </summary>
        public string? uri { get; set; }

        /// <summary>
        /// attemptedDate
        /// </summary>
        public DateTime? attemptedDate { get; set; }

        /// <summary>
        /// attemptNumber
        /// </summary>
        public int? attemptNumber { get; set; }

        /// <summary>
        /// attemptedAmount
        /// </summary>
        public double? attemptedAmount { get; set; }

        /// <summary>
        /// paymentMethodType
        /// </summary>
        public string? paymentMethodType { get; set; }

        /// <summary>
        /// paymentMethod
        /// </summary>
        public string? paymentMethod { get; set; }

        /// <summary>
        /// result
        /// </summary>
        public string? result { get; set; }

        /// <summary>
        /// gateway
        /// </summary>
        public string? gateway { get; set; }

        /// <summary>
        /// gatewayAuthResponse
        /// </summary>
        public string? gatewayAuthResponse { get; set; }

        /// <summary>
        /// gatewayAuthCode
        /// </summary>
        public string? gatewayAuthCode { get; set; }

        /// <summary>
        /// gatewaySecondaryResponse
        /// </summary>
        public string? gatewaySecondaryResponse { get; set; }

        /// <summary>
        /// gatewayPaymentPlatformCode
        /// </summary>
        public string? gatewayPaymentPlatformCode { get; set; }

        /// <summary>
        /// ReconciliationId
        /// </summary>
        public string? ReconciliationId { get; set; }
    }

    public class InvoiceAllocation
    {
        /// <summary>
        /// invoiceId
        /// </summary>
        public int invoiceId { get; set; }

        /// <summary>
        /// invoiceNumber
        /// </summary>
        public int invoiceNumber { get; set; }

        /// <summary>
        /// amount
        /// </summary>
        public double amount { get; set; }

        /// <summary>
        /// outstandingBalance
        /// </summary>
        public double outstandingBalance { get; set; }

        /// <summary>
        /// uri
        /// </summary>
        public string uri { get; set; }

    }

    public class Refund
    {
        /// <summary>
        /// createdTimestamp
        /// </summary>
        public DateTime? createdTimestamp { get; set; }

        /// <summary>
        /// paymentSource
        /// </summary>
        public string? paymentSource { get; set; }

        /// <summary>
        /// paymentActivityId
        /// </summary>
        public int? paymentActivityId { get; set; }

        /// <summary>
        /// reference
        /// </summary>
        public string? reference { get; set; }

        /// <summary>
        /// effectiveTimestamp
        /// </summary>
        public DateTime? effectiveTimestamp { get; set; }

        /// <summary>
        /// description
        /// </summary>
        public string? description { get; set; }

        /// <summary>
        /// customerId
        /// </summary>
        public int? customerId { get; set; }

        /// <summary>
        /// originalPaymentActivityId
        /// </summary>
        public int? originalPaymentActivityId { get; set; }

        /// <summary>
        /// amount
        /// </summary>
        public double? amount { get; set; }

        /// <summary>
        /// currency
        /// </summary>
        public string? currency { get; set; }

        /// <summary>
        /// invoiceAllocations
        /// </summary>
        public List<InvoiceAllocation>? invoiceAllocations { get; set; }

        /// <summary>
        /// unallocatedAmount
        /// </summary>
        public double? unallocatedAmount { get; set; }

        /// <summary>
        /// refundableAmount
        /// </summary>
        public double? refundableAmount { get; set; }

        /// <summary>
        /// id
        /// </summary>
        public int? id { get; set; }

        /// <summary>
        /// uri
        /// </summary>
        public string uri { get; set; }
    }

    public class FuseBillPaymentResponse
    {
        public string? SPResult { get; set; }
    }

    public class PaymentMethod
    {
        /// <summary>
        /// customerId
        /// </summary>
        public int? customerId { get; set; }
    }
}
