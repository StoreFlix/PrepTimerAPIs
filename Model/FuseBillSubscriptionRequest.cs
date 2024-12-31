using System;

namespace ServiceFabricApp.API.Model
{
    public class FuseBillSubscriptionRequest
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
        /// Subscription
        /// </summary>
        public Subscription? Subscription { get; set; }
    }

    public class Subscription
    {
        public int customerId { get; set; }
        public string? planCode { get; set; }
        public string? planName { get; set; }
        public string? planReference { get; set; }
        public string? planDescription { get; set; }
        public string? status { get; set; }
        public string? reference { get; set; }
        public DateTime? createdTimestamp { get; set; }
        public DateTime? activatedTimestamp { get; set; }
        public DateTime? provisionedTimestamp { get; set; }
        public DateTime? nextPeriodStartDate { get; set; }
        public DateTime? scheduledActivationTimestamp { get; set; }
        public string? remainingInterval { get; set; }
        public string? subscriptionOverride { get; set; }
        public string? frequency { get; set; }
        public double? monthlyRecurringRevenue { get; set; }
        public double? netMrr { get; set; }
        public string? oneTimeRevenue { get; set; }
        public string? deferredRevenue { get; set; }
        public double? amount { get; set; }
        public string? salesforceId { get; set; }
        public int? id { get; set; }
        public string? uri { get; set; }
    }

    public class FuseBillSubscriptionResponse
    {
        public string? SPResult { get; set; }
    }

}
