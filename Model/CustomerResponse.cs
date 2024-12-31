using System;
using System.Collections.Generic;
using System.Net;

namespace ServiceFabricApp.API.Model
{
    public class CustomerResponse
    {

    }


    /// <summary>
    /// FusebillCustomerResponse
    /// </summary>
    public class FusebillCustomerResponse
    {
        /// <summary>
        /// CustomerId
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// PaymentUrl
        /// </summary>
        public string PaymentUrl { get; set; }

        /// <summary>
        /// PaymentToken
        /// </summary>
        public string PaymentToken { get; set; }
    }


    /// <summary>
    /// CustomerDropDowns
    /// </summary>
    public class CustomerDropDowns
    {
        /// <summary>
        /// Countries
        /// </summary>
        public List<CustomerCountries> Countries { get; set; }

        /// <summary>
        /// Franchises
        /// </summary>
        public List<CustomerFranchise> Franchises { get; set; }

        public List<TimeZoneInfo> TimezoneList { get; set; }
        
    }


    public class CustomerCountries
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Iso3
        /// </summary>
        public string Iso3 { get; set; }

        /// <summary>
        /// states
        /// </summary>
        public List<states> states { get; set; }
    }

    public class states
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// SubDivisionIsoCode
        /// </summary>
        public string SubDivisionIsoCode { get; set; }
    }


    /// <summary>
    /// CustomerFranchise
    /// </summary>
    public class CustomerFranchise
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }
    }


    /// <summary>
    /// CustomerSubscriptionResponse
    /// </summary>
    public class CustomerSubscriptionResponse
    {
        /// <summary>
        /// ProductId
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// ProductCode
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// ProductName
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// ProductDescription
        /// </summary>
        public string ProductDescription { get; set; }

        public int SubscriptionId { get; set; }

        /// <summary>
        /// Plans
        /// </summary>
        public List<CustomerSubscriptionPlan> Plans { get; set; }
    }


    /// <summary>
    /// CustomerSubscriptionPlan
    /// </summary>
    public class CustomerSubscriptionPlan
    {

        /// <summary>
        /// PlanId
        /// </summary>
        public int PlanId { get; set; }

        /// <summary>
        /// PlanCode
        /// </summary>
        public string PlanCode { get; set; }

        /// <summary>
        /// PlanPlanName
        /// </summary>
        public string PlanName { get; set; }

        /// <summary>
        /// PlanDescription
        /// </summary>
        public string PlanDescription { get; set; }

        /// <summary>
        /// PlanFrequencyId
        /// </summary>
        public string PlanFrequencyId { get; set; }

        /// <summary>
        /// PlanProductUniqueId
        /// </summary>
        public string PlanProductUniqueId { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        public string Currency { get; set; }
    }


    /// <summary>
    /// FuseBillProduct
    /// </summary>
    public class FuseBillProduct
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Uri
        /// </summary>
        public string Uri { get; set; }
    }


    /// <summary>
    /// FusebillPlan
    /// </summary>
    public class FusebillPlan
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; set; }
    }

    public class FusebillSubscription
    {
        public int customerId { get; set; }
        public int planFrequencyId { get; set; }
        public List<string> couponCodes { get; set; }
        public List<SubscriptionProducts> subscriptionProducts { get; set; }
    }


    public class CustomerSubscriptionDetailResponse
    {
        public string FranchaiseCode { get; set; }
        public List<CustomerSubscriptions> customerSubscriptions { get; set; }

    }

    /// <summary>
    /// CustomerSubscriptions
    /// </summary>
    public class CustomerSubscriptions
    {
        public int SubscriptionId { get; set; }
        public int SubscriptionProductId { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public int PlanId { get; set; }
        public string PlanName { get; set; }
        public int PlanFrequencyId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int PlanProductUniqueId { get; set; }
        public DateTime NextRechargeDate { get; set; }
        public DateTime LastPurchaseDate { get; set; }
        public DateTime SubscriptionDetailDate { get; set; }
        public List<CouponDetail> couponDetail { get; set; }
    }

    public class CouponDetail
    {
        public int couponId { get; set; }
        public string couponCode { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public DateTime? eligibilityStartDate { get; set; }
        public DateTime? eligibilityEndDate { get; set; }
        public string status { get; set; }
    }

    public class FusebillInvoiceRes
    {
        public List<FusebillInvoiceResponse> fusebillInvoiceResponse { get; set; }

        public FusebillErrorMessage fusebillErrorMessage { get; set; }
    }

    public class FusebillInvoiceResponse
    {
        public int customerId { get; set; }
        public int planFrequencyId { get; set; }
        public string planCode { get; set; }
        public string planName { get; set; }
        public string planDescription { get; set; }
        public string planReference { get; set; }
        public string status { get; set; }
        public string reference { get; set; }
        public SideEffects sideEffects { get; set; }
    }

    public class SideEffects
    {
        public DraftInvoice draftInvoice { get; set; }
    }

    public class DraftInvoice
    {
        public string status { get; set; }
        public List<DraftCharges> draftCharges { get; set; }
        public float subtotal { get; set; }
        public float totalDiscount { get; set; }
        public float total { get; set; }
        public Tax? tax { get; set; }

    }

    public class Tax
    {
        public float total { get; set; }
    }
    public class DraftCharges
    {
        public double quantity { get; set; }
        public float unitPrice { get; set; }
        public float amount { get; set; }
        public float taxableAmount { get; set; }
        public DraftDiscount draftDiscount { get; set; }
        public List<DraftDiscount> draftDiscounts { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int productId { get; set; }
        public string status { get; set; }
    }

    public class DraftDiscount
    {
        public float configuredDiscountAmount { get; set; }
        public float amount { get; set; }
        public double quantity { get; set; }
        public string discountType { get; set; }
        public string description { get; set; }
    }

    public class FusebillCouponResponse
    {
        public bool valid { get; set; }
        public string reason { get; set; }
    }

    public class FusebillDraftInvoiceResp
    {
        public List<FusebillDraftInvoiceResponse> fusebillDraftInvoiceResponse { get; set; }
        public float subtotal { get; set; }
        public float totalDiscount { get; set; }
        public float total { get; set; }
        public Tax? taxes { get; set; }
        public FusebillErrorMessage fusebillErrorMessage { get; set; }
    }

    public class FusebillDraftInvoiceResponse
    {
        public int productId { get; set; }
        public string productName { get; set; }
        public string productDescription { get; set; }
        public string planCode { get; set; }
        public string planName { get; set; }
        public string planDescription { get; set; }
        public double quantity { get; set; }
        public float unitPrice { get; set; }
        public float amount { get; set; }
        public List<Discounts> discounts { get; set; }
        public float taxableAmount { get; set; }
    }

    public class Discounts
    {
        public float configuredDiscountAmount { get; set; }
        public float discountAmount { get; set; }
        public string discountType { get; set; }
        public string discountDescription { get; set; }
    }

    public class FusebillSubscriptionResp
    {
        public string message { get; set; }
        public HttpStatusCode status { get; set; }
    }


    public class FusebillCustomerDetail
    {
        public int CustomerId { get; set; }
        public bool IsCustomerExists { get; set; }
        public int PaymentActivityId { get; set; }
        public string EventType { get; set; }
        public decimal Amount { get; set; }
        public int TransactionId { get; set; }
        public string InvoiceAllocations { get; set; }
        public bool PaymentMethodExist { get; set; }
    }

    public class RefundAllocations
    {
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; }
    }

    public class FusebillErrorMessage
    {
        public List<ErrorMessage> errorMessage { get; set; }
    }

    public class ErrorMessage
    {
        public string message { get; set; }
        public HttpStatusCode status { get; set; }
    }


    public class FusebillSSOUrl
    {
        public string SSOUrl { get; set; }
    }

    public class Subscriptions
    {
        public string SubscriptionName { get; set; }
        public int SubscriptionId { get; set; }
    }

    public class FusebillProductSubscriptions
    {
        public string FranchiseCode { get; set; }
        public int FusebillProductId { get; set; }
        public string FusebillProductName { get; set; }
        public int SubscriptionId { get; set; }
        public string SubscriptionType { get; set; }
        public int EquipmentTypeId { get; set; }
    }

    public class SubscriptionModel
    {
        public int companyId { get; set; }
        public List<SubscriptionKeyValue> subscriptionList { get; set; }
    }
    public class SubscriptionKeyValue
    {
        public string Key { get; set; }
        public bool Value { get; set; }
    }

    public class SubscriptionType
    {
        public int Id { get; set; }
        public string Subscription_Type { get; set; }
        public int CSId { get; set; }
        public int CompanyId { get; set; }
        public int SubscriptionId { get; set; }
        public bool IsChecked { get; set; }
        public string Key { get; set; }
    }

    public class CompanyPaymentAddress
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public string Currency { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
    }

    public class CompanyDetail
    {
        public int FusebillID { get; set; }
        public bool IsFusebillEnable { get; set; }
        public string FranchaiseCode { get; set; }
    }

    public class CompanyDetails
    {
        public int CompanyId { get; set; }
        public string FranchaiseCode { get; set; }
        public bool IsFusebillEnable { get; set; }
    }
}

