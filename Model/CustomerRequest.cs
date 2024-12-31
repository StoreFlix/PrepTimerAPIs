using System.Collections.Generic;

namespace ServiceFabricApp.API.Model
{
    public class CustomerRequest
    {

    }


    /// <summary>
    /// CustomerDropDownRequest
    /// </summary>
    public class CustomerDropDownRequest
    {
        /// <summary>
        /// IsCountries
        /// </summary>
        public bool IsCountries { get; set; }

        /// <summary>
        /// IsFranchise
        /// </summary>
        public bool IsFranchise { get; set; }

    }


    /// <summary>
    /// CustomerRegistrationRequest
    /// </summary>
    public class CustomerRegistrationRequest
    {
        /// <summary>
        /// RegisterCompany
        /// </summary>
        public List<RegisterCompany> RegisterCompany { get; set; }

        /// <summary>
        /// RegisterStore
        /// </summary>
        public List<RegisterStore> RegisterStore { get; set; }

        /// <summary>
        /// RegisterUser
        /// </summary>
        public List<RegisterUser> RegisterUser { get; set; }

        /// <summary>
        /// RegisterSubscription
        /// </summary>
        public List<RegisterSubscription> RegisterSubscription { get; set; }
    }


    /// <summary>
    /// RegisterCompany
    /// </summary>
    public class RegisterCompany
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
        /// Phone
        /// </summary>
        public string Phone { get; set; }


        /// <summary>
        /// Address1
        /// </summary>
        public string Address1 { get; set; }


        /// <summary>
        /// Address2
        /// </summary>
        public string Address2 { get; set; }


        /// <summary>
        /// City
        /// </summary>
        public string City { get; set; }


        /// <summary>
        /// Currency
        /// </summary>
        public string Currency { get; set; }


        /// <summary>
        /// State
        /// </summary>
        public string State { get; set; }


        /// <summary>
        /// Zipcode
        /// </summary>
        public string Zipcode { get; set; }


        /// <summary>
        /// County
        /// </summary>
        public string County { get; set; }


        /// <summary>
        /// Country
        /// </summary>
        public string Country { get; set; }


        /// <summary>
        /// LogoFile
        /// </summary>
        public string LogoFile { get; set; }


        /// <summary>
        /// IsCommercial
        /// </summary>
        public bool IsCommercial { get; set; }
    }

    /// <summary>
    /// RegisterStore
    /// </summary>
    public class RegisterStore
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// RegionName
        /// </summary>
        public string RegionName { get; set; }


        /// <summary>
        /// TerritoryName
        /// </summary>
        public string TerritoryName { get; set; }


        /// <summary>
        /// SubTerritoryName
        /// </summary>
        public string SubTerritoryName { get; set; }


        /// <summary>
        /// StoreNumber
        /// </summary>
        public string StoreNumber { get; set; }


        /// <summary>
        /// Phone
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Address1
        /// </summary>
        public string Address1 { get; set; }


        /// <summary>
        /// Address2
        /// </summary>
        public string Address2 { get; set; }


        /// <summary>
        /// City
        /// </summary>
        public string City { get; set; }


        /// <summary>
        /// State
        /// </summary>
        public string State { get; set; }


        /// <summary>
        /// Zipcode
        /// </summary>
        public string Zipcode { get; set; }


        /// <summary>
        /// County
        /// </summary>
        public string County { get; set; }


        /// <summary>
        /// Country
        /// </summary>
        public string Country { get; set; }


        /// <summary>
        /// Latitude
        /// </summary>
        public string Latitude { get; set; }


        /// <summary>
        /// Longitude
        /// </summary>
        public string Longitude { get; set; }


        /// <summary>
        /// TimeZone
        /// </summary>
        public string TimeZone { get; set; }


        /// <summary>
        /// UTCOffSet
        /// </summary>
        public int UTCOffSet { get; set; }
    }

    /// <summary>
    /// RegisterUser
    /// </summary>
    public class RegisterUser
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// FirstName
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// LastName
        /// </summary>
        public string LastName { get; set; }


        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }


        /// <summary>
        /// LandingPage
        /// </summary>
        public int LandingPage { get; set; }


        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; set; }


        /// <summary>
        /// Phone1
        /// </summary>
        public string Phone1 { get; set; }


        /// <summary>
        /// Phone2
        /// </summary>
        public string Phone2 { get; set; }


        /// <summary>
        /// UserType
        /// </summary>
        public string UserType { get; set; }
    }

    /// <summary>
    /// RegisterSubscription
    /// </summary>
    public class RegisterSubscription
    {
        /// <summary>
        /// SubscriptionId
        /// </summary>
        public int SubscriptionId { get; set; }

    }

    public class FusebillInvoiceReq
    {
        public List<FusebillInvoiceRequest> fusebillInvoiceRequest { get; set; }

    }

    public class TaxationAddress
    {
        public String? Line1 { get; set; }
        public String? Line2 { get; set; }

        public int countryId { get; set; }
        public int stateId { get;set; }
        public String? city { get; set; }   
        public String? postalZip { get; set; }
    }



    public class FusebillInvoiceRequest
    {
        public int planId { get; set; }
        public int planFrequencyId { get; set; }
        public List<string> couponCodes { get; set; }
        public List<SubscriptionProducts> subscriptionProducts { get; set; }
        public TaxationAddress? taxationAddress { get; set; }

    }

    public class SubscriptionProducts
    {
        public int planProductUniqueId { get; set; }
        public double quantity { get; set; }
        public bool isIncluded { get; set; }
    }
    public class FusebillCouponRequest
    {
        public string couponCode { get; set; }
        public int planId { get; set; }
    }

    public class FusebillSubscriptionReq
    {
        public int customerId { get; set; }

        public string FranchiseCode { get; set; }

        public int paymentActivityId { get; set; }

        public CustomerRegistrationRequest customerRegistrationRequest { get; set; }

        public List<FusebillSubscriptionRequest> fusebillSubscriptionRequest { get; set; }
    }

    public class FusebillSubscriptionRequest
    {
        public int customerId { get; set; }
        public int planFrequencyId { get; set; }
        public List<string> couponCodes { get; set; }
        public List<SubscriptionProducts> subscriptionProducts { get; set; }
    }

    public class FusebillCustomerRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string PrimaryEmail { get; set; }
        public string Currency { get; set; }
        public Billing billing { get; set; }
    }

    public class Billing
    {
        public string CompanyName { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public int PostalZip { get; set; }
        public string Country { get; set; }
        public string state { get; set; }

    }

}
