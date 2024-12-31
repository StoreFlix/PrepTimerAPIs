using Azure.Storage;
using Azure.Storage.Blobs;

using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ServiceFabricApp.API.Model;
using ServiceFabricApp.API.Repositories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;


namespace ServiceFabricApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {

        /// <summary>
        /// Configuration
        /// </summary>
        private readonly IConfiguration Configuration;


        /// <summary>
        /// CustomerController
        /// </summary>
        /// <param name="_configuration"></param>
        public CustomerController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }


        [EnableCors("customPolicy")]
        [Route("AddFusebillCustomer")]
        [HttpPost]
        public FusebillCustomerResponse AddFusebillCustomer(FusebillCustomerRequest fusebillCustomerRequest)
        {
            FusebillCustomerResponse fusebillCustomerResponse = new FusebillCustomerResponse();
            try
            {
                //InCase request object is Null nothing to execute
                if (fusebillCustomerRequest != null)
                {
                    string jsonData = JsonConvert.SerializeObject(fusebillCustomerRequest);

                    //Getting the connection string from appsettings.json which is included in Starup.cs
                    string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                    //Sql Connection to pass request and fetch response  
                    using (SqlConnection connection = new SqlConnection(strSqlConnection))
                    {
                        //Calling the stored procedure with all the required paramaters 
                        SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerGetFuseBillKeyValueProcedure, connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 300;

                        cmd.Parameters.Add("@KeyName", SqlDbType.NVarChar).Value = "'APIKey', 'CreateCustomerUrl', 'FusebillPaymentUrl', 'FusebillPaymentToken'";

                        connection.Open();
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dtFusebillKey = new DataTable())
                            {
                                //Moving the SQL response into the DataTable
                                sda.Fill(dtFusebillKey);

                                var dataRowApiKey = dtFusebillKey.AsEnumerable().Where(x => x.Field<string>("KeyName") == "APIKey").FirstOrDefault();
                                string apiKey = Convert.ToString(dataRowApiKey["Value"]);

                                var dataRowCreateCustomerUrl = dtFusebillKey.AsEnumerable().Where(x => x.Field<string>("KeyName") == "CreateCustomerUrl").FirstOrDefault();
                                string createCustomerUrl = Convert.ToString(dataRowCreateCustomerUrl["Value"]);

                                var dataRowFusebillPaymentUrl = dtFusebillKey.AsEnumerable().Where(x => x.Field<string>("KeyName") == "FusebillPaymentUrl").FirstOrDefault();
                                fusebillCustomerResponse.PaymentUrl = Convert.ToString(dataRowFusebillPaymentUrl["Value"]);

                                var dataRowFusebillPaymentToken = dtFusebillKey.AsEnumerable().Where(x => x.Field<string>("KeyName") == "FusebillPaymentToken").FirstOrDefault();
                                fusebillCustomerResponse.PaymentToken = Convert.ToString(dataRowFusebillPaymentToken["Value"]);

                                WebRequest request = WebRequest.Create(createCustomerUrl);
                                request.ContentType = "application/json";
                                request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                                request.Method = "POST";
                                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                                {
                                    streamWriter.Write(jsonData);
                                    streamWriter.Flush();
                                    streamWriter.Close();
                                }
                                var httpResponse = (HttpWebResponse)request.GetResponse();
                                var result = "";
                                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    result = streamReader.ReadToEnd();
                                    dynamic data = JObject.Parse(result);

                                    fusebillCustomerResponse.CustomerId = Convert.ToInt32(data.id);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "AddFusebillCustomer", ex.Message);
            }
            return fusebillCustomerResponse;
        }

        [EnableCors("customPolicy")]
        [Route("GetCompanyFusebillId")]
        [HttpPost]
        public CompanyDetail GetCompanyFusebillId()
        {
            //int fusebillId = 0;
            int companyId = 0;
            CompanyDetail companyDetail = new CompanyDetail();
            if (!String.IsNullOrEmpty(Request.Headers["CompanyId"]))
            {
                companyId = Convert.ToInt32(Request.Headers["CompanyId"]);
            }

            string? clientName = string.Empty;
            if(User != null)
            {
                clientName = User.FindFirst("clientName")?.Value;
            }
     
            try
            {
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                //Sql Connection to pass request and fetch response  
                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand("PT_RegisterCustomerCheckFusebillId", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@ClientName", SqlDbType.VarChar).Value = clientName;

                    connection.Open();
                    using (SqlDataReader dbReader = cmd.ExecuteReader())
                        while (dbReader.Read())
                        {
                            companyDetail.FusebillID = Convert.ToInt32(dbReader["FusebillId"]);
                            companyDetail.FranchaiseCode = Convert.ToString(dbReader["FranchiseCode"]);
                            companyDetail.IsFusebillEnable = Convert.ToBoolean(dbReader["IsFusebillEnable"]);
                        }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "GetCompanyFusebillId", ex.Message);
                throw ex;
            }
            return companyDetail;
        }

        [EnableCors("customPolicy")]
        [Route("CheckCustomerCompanyName")]
        [HttpPost]
        public bool CheckCustomerCompanyName(string strCustomerCompanyName)
        {
            bool isCompanyNameExists = true;
            try
            {
                //InCase request object is Null nothing to execute
                if (!string.IsNullOrEmpty(strCustomerCompanyName))
                {
                    //Getting the connection string from appsettings.json which is included in Starup.cs
                    string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                    //Sql Connection to pass request and fetch response  
                    using (SqlConnection connection = new SqlConnection(strSqlConnection))
                    {
                        //Calling the stored procedure with all the required paramaters 
                        SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerCheckCompanyNameProcedure, connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 300;

                        cmd.Parameters.Add("@CompanyName", SqlDbType.NVarChar).Value = strCustomerCompanyName;

                        connection.Open();
                        using (SqlDataReader dbReader = cmd.ExecuteReader())
                            while (dbReader.Read())
                                isCompanyNameExists = Convert.ToInt32(dbReader["Count"]) > 0 ? true : false;
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "CheckCustomerCompanyName", ex.Message);
            }
            return isCompanyNameExists;
        }


        [EnableCors("customPolicy")]
        [Route("CheckCustomerLoginName")]
        [HttpPost]
        public bool CheckCustomerLoginName(string strCustomerLoginName)
        {
            bool isLoginNameExists = true;
            try
            {
                //InCase request object is Null nothing to execute
                if (!string.IsNullOrEmpty(strCustomerLoginName))
                {
                    //Getting the connection string from appsettings.json which is included in Starup.cs
                    string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                    //Sql Connection to pass request and fetch response  
                    using (SqlConnection connection = new SqlConnection(strSqlConnection))
                    {
                        //Calling the stored procedure with all the required paramaters 
                        SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerCheckLoginNameProcedure, connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 300;

                        cmd.Parameters.Add("@LoginName", SqlDbType.NVarChar).Value = strCustomerLoginName;

                        connection.Open();
                        using (SqlDataReader dbReader = cmd.ExecuteReader())
                            while (dbReader.Read())
                                isLoginNameExists = Convert.ToInt32(dbReader["Count"]) > 0 ? true : false;
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "CheckCustomerLoginName", ex.Message);
            }
            return isLoginNameExists;
        }


        [EnableCors("customPolicy")]
        [Route("GetCompanyPaymentAddress")]
        [HttpGet]
        public CompanyPaymentAddress GetCompanyPaymentAddress()
        {
            CompanyPaymentAddress companyPaymentAddress = new CompanyPaymentAddress();
            try
            {
                //companyId will get it from header token
                int companyId = 0;
                if (!String.IsNullOrEmpty(Request.Headers["CompanyId"]))
                    companyId = Convert.ToInt32(Request.Headers["CompanyId"]);

                //UserId will get it from header token
                int userId = 0;
                if (!String.IsNullOrEmpty(Request.Headers["UserId"]))
                    userId = Convert.ToInt32(Request.Headers["UserId"]);

                string? clientName = string.Empty;
                if (User != null)
                {
                    clientName = User.FindFirst("clientName")?.Value;
                }

                //InCase request object is Null nothing to execute
                if (!String.IsNullOrWhiteSpace(clientName))
                {
                    //Getting the connection string from appsettings.json which is included in Starup.cs
                    string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                    //Sql Connection to pass request and fetch response  
                    using (SqlConnection connection = new SqlConnection(strSqlConnection))
                    {
                        //Calling the stored procedure with all the required paramaters 
                        SqlCommand cmd = new SqlCommand("PT_RegisterCompanyPaymentAddress", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 300;

                        cmd.Parameters.Add("@ClientName", SqlDbType.VarChar).Value = clientName;

                        connection.Open();
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                //Moving the SQL response into the DataTable
                                sda.Fill(dt);
                                companyPaymentAddress = Common.ConvertDataTable<CompanyPaymentAddress>(dt).FirstOrDefault();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "GetCompanyPaymentAddress", ex.Message);
            }
            return companyPaymentAddress;
        }


        [EnableCors("customPolicy")]
        [Route("GetCustomerDropDown")]
        [HttpPost]
        public CustomerDropDowns GetCustomerDropDown(CustomerDropDownRequest objCustomerDropDownRequest)
        {
            CustomerDropDowns objCustomerDropDowns = new CustomerDropDowns();
            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                //Sql Connection to pass request and fetch response  
                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {

                    connection.Open();

                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerGetFuseBillKeyValueProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;
                    cmd.Parameters.Add("@KeyName", SqlDbType.NVarChar).Value = "'APIKey', 'CountriesUrl', 'FranchiseUrl'";
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            var dataRowApiKey = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "APIKey").FirstOrDefault();
                            string apiKey = Convert.ToString(dataRowApiKey["Value"]);

                            WebRequest request = null;
                            var result = "";

                            if (objCustomerDropDownRequest.IsCountries)
                            {
                                var dataRowCountriesUrl = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "CountriesUrl").FirstOrDefault();
                                string countriesUrl = Convert.ToString(dataRowCountriesUrl["Value"]);

                                //Configure URI
                                request = WebRequest.Create(countriesUrl);
                                //Add Content type
                                request.ContentType = "application/json";
                                //Add Api key authorization
                                request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                                //Set request method
                                request.Method = "GET";
                                //Perform the request
                                var httpResponse = (HttpWebResponse)request.GetResponse();
                                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    result = streamReader.ReadToEnd();
                                    objCustomerDropDowns.Countries = JsonConvert.DeserializeObject<List<CustomerCountries>>(result);
                                }
                            }
                            if (objCustomerDropDownRequest.IsFranchise)
                            {
                                cmd = new SqlCommand(Keys.RegisterCustomerGetCustomerFranchisesProcedure, connection);
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandTimeout = 300;

                                using (SqlDataAdapter sdaF = new SqlDataAdapter(cmd))
                                {
                                    using (DataTable dtF = new DataTable())
                                    {
                                        //Moving the SQL response into the DataTable
                                        sdaF.Fill(dtF);

                                        objCustomerDropDowns.Franchises = Common.ConvertDataTable<CustomerFranchise>(dtF);
                                    }
                                }
                            }

                            objCustomerDropDowns.TimezoneList = TimeZoneInfo.GetSystemTimeZones().ToList();
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "GetCustomerDropDown", ex.Message);
            }
            return objCustomerDropDowns;
        }

        [EnableCors("customPolicy")]
        [Route("GetCustomerSubscriptions")]
        [HttpGet]
        public List<CustomerSubscriptionResponse> GetCustomerSubscriptions(string FranchiseCode)
        {
            List<CustomerSubscriptionResponse> lstCustomerSubscriptionResponse = new List<CustomerSubscriptionResponse>();
            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                //Sql Connection to pass request and fetch response  
                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerGetFuseBillKeyValueProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@KeyName", SqlDbType.NVarChar).Value = "'APIKey', 'ProductsUrl', 'PlansUrl', 'ProductPlansUrl'";

                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            var dataRowApiKey = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "APIKey").FirstOrDefault();
                            string apiKey = Convert.ToString(dataRowApiKey["Value"]);


                            var dataRowProductsUrl = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "ProductsUrl").FirstOrDefault();
                            string productsUrl = Convert.ToString(dataRowProductsUrl["Value"]);
                            var dataRowPlansUrl = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "PlansUrl").FirstOrDefault();
                            string plansUrl = Convert.ToString(dataRowPlansUrl["Value"]);
                            var dataRowProductPlansUrl = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "ProductPlansUrl").FirstOrDefault();
                            string productPlansUrl = Convert.ToString(dataRowProductPlansUrl["Value"]);

                            List<FuseBillProduct> productFranchiseResponse = new List<FuseBillProduct>();
                            List<FuseBillProduct> productResponse = new List<FuseBillProduct>();
                            List<FusebillPlan> planResponse = new List<FusebillPlan>();


                            //Configure URI
                            WebRequest request = WebRequest.Create(productsUrl);
                            //Add Content type
                            request.ContentType = "application/json";
                            //Add Api key authorization
                            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                            //Set request method
                            request.Method = "GET";
                            //Perform the request
                            var httpResponse = (HttpWebResponse)request.GetResponse();
                            //Record the response from our request
                            var result = "";
                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                result = streamReader.ReadToEnd();
                                productResponse = JsonConvert.DeserializeObject<List<FuseBillProduct>>(result);
                            }



                            foreach (FuseBillProduct fuseBillProduct in productResponse)
                            {
                                //Configure URI
                                request = WebRequest.Create(fuseBillProduct.Uri);
                                //Add Content type
                                request.ContentType = "application/json";
                                //Add Api key authorization
                                request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                                //Set request method
                                request.Method = "GET";
                                //Perform the request
                                httpResponse = (HttpWebResponse)request.GetResponse();
                                //Record the response from our request
                                result = "";
                                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    result = streamReader.ReadToEnd();
                                    JsonDocument doc = JsonDocument.Parse(result);
                                    JsonElement root = doc.RootElement;
                                    var u1 = root;
                                    JsonElement jeKeyExist = new JsonElement();
                                    bool isFieldExist = u1.TryGetProperty("productCustomFields", out jeKeyExist);
                                    if (isFieldExist)
                                    {
                                        // Modified logic to enable multiple coupon codes
                                        for (int i = 0; i < root.GetProperty("productCustomFields").GetArrayLength(); i++)                                            
                                        {
                                            string strProductKey = u1.GetProperty("productCustomFields")[i].GetProperty("key").ToString();
                                            if (FranchiseCode.ToLower() == strProductKey.ToLower())
                                                productFranchiseResponse.Add(fuseBillProduct);
                                        }
                                    }
                                    else if (FranchiseCode.ToLower() == "Others".ToLower() && !isFieldExist)
                                    {
                                        productFranchiseResponse.Add(fuseBillProduct);
                                    }
                                }
                            }



                            request = WebRequest.Create(plansUrl);
                            //Add Content type
                            request.ContentType = "application/json";
                            //Add Api key authorization
                            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                            //Set request method
                            request.Method = "GET";
                            //Perform the request
                            httpResponse = (HttpWebResponse)request.GetResponse();
                            //Record the response from our request
                            result = "";
                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                result = streamReader.ReadToEnd();
                                planResponse = JsonConvert.DeserializeObject<List<FusebillPlan>>(result);
                            }

                            List<Subscriptions> subscriptions = GetActiveSubscriptions();
                            List<FusebillProductSubscriptions> fusebillSubscriptions = GetFusebillProductSubscriptions(FranchiseCode);
                            foreach (FuseBillProduct product in productFranchiseResponse)
                            {
                                CustomerSubscriptionResponse objCustomerSubscriptionResponse = new CustomerSubscriptionResponse();
                                objCustomerSubscriptionResponse.ProductId = product.Id;
                                objCustomerSubscriptionResponse.ProductName = product.Name;
                                objCustomerSubscriptionResponse.ProductCode = product.Code;
                                objCustomerSubscriptionResponse.ProductDescription = product.Description;
                                
                                // Mapped Subscription Id                               
                                if (subscriptions.Count > 0)
                                    objCustomerSubscriptionResponse.SubscriptionId = subscriptions.Where(t => t.SubscriptionName == objCustomerSubscriptionResponse.ProductName).Select(n => n.SubscriptionId).FirstOrDefault();
                                // Logic for equipment to set id
                                if (objCustomerSubscriptionResponse.SubscriptionId == 0 && fusebillSubscriptions.Count > 0)                                
                                    objCustomerSubscriptionResponse.SubscriptionId = fusebillSubscriptions.Where(t => t.FusebillProductId == objCustomerSubscriptionResponse.ProductId).Select(n => n.SubscriptionId).FirstOrDefault();                                

                                List<CustomerSubscriptionPlan> lstCustomerSubscriptionPlan = new List<CustomerSubscriptionPlan>();
                                string strProductPlanUrl = productPlansUrl.Replace("{productId}", product.Id.ToString());
                                request = WebRequest.Create(strProductPlanUrl);
                                //Add Content type
                                request.ContentType = "application/json";
                                //Add Api key authorization
                                request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                                //Set request method
                                request.Method = "GET";
                                //Perform the request
                                httpResponse = (HttpWebResponse)request.GetResponse();
                                //Record the response from our request
                                result = "";
                                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                {

                                    result = streamReader.ReadToEnd();
                                    JsonDocument doc = JsonDocument.Parse(result);
                                    JsonElement root = doc.RootElement;
                                    for (int i = 0; i < root.GetArrayLength(); i++)
                                    {
                                        CustomerSubscriptionPlan objCustomerSubscriptionPlan = new CustomerSubscriptionPlan();

                                        var u1 = root[i];
                                        JsonElement jeplanFrequencyId = u1.GetProperty("orderToCashCycles")[0].GetProperty("planFrequencyId");
                                        JsonElement jeplanProductUniqueId = u1.GetProperty("planProductUniqueId");
                                        JsonElement jePlanId = u1.GetProperty("planId");
                                        JsonElement jeAmount = u1.GetProperty("orderToCashCycles")[0].GetProperty("pricingModel").GetProperty("quantityRanges")[0].GetProperty("prices")[0].GetProperty("amount");
                                        JsonElement jeCurrency = u1.GetProperty("orderToCashCycles")[0].GetProperty("pricingModel").GetProperty("quantityRanges")[0].GetProperty("prices")[0].GetProperty("currency");

                                        int planId = Convert.ToInt32(jePlanId.ToString());
                                        FusebillPlan planRow = planResponse.Where(item => item.Id == planId).FirstOrDefault();
                                        if (planRow != null)
                                        {
                                            objCustomerSubscriptionPlan.PlanId = planRow.Id;
                                            objCustomerSubscriptionPlan.PlanCode = planRow.Code;
                                            objCustomerSubscriptionPlan.PlanName = planRow.Name;
                                            objCustomerSubscriptionPlan.PlanDescription = planRow.Description;
                                            objCustomerSubscriptionPlan.PlanFrequencyId = jeplanFrequencyId.ToString();
                                            objCustomerSubscriptionPlan.PlanProductUniqueId = jeplanProductUniqueId.ToString();
                                            objCustomerSubscriptionPlan.Price = jeAmount.ToString();
                                            objCustomerSubscriptionPlan.Currency = jeCurrency.ToString();

                                            lstCustomerSubscriptionPlan.Add(objCustomerSubscriptionPlan);
                                        }

                                    }
                                }
                                objCustomerSubscriptionResponse.Plans = lstCustomerSubscriptionPlan;

                                lstCustomerSubscriptionResponse.Add(objCustomerSubscriptionResponse);
                            }

                            lstCustomerSubscriptionResponse= lstCustomerSubscriptionResponse.OrderByDescending((a) => a.ProductName.Contains("Flow Totalizer")).ToList();

                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "GetCustomerSubscriptions", ex.Message);
            }
            return lstCustomerSubscriptionResponse;
        }

        [EnableCors("customPolicy")]
        [Route("GetInvoiceForPlan")]
        [HttpPost]
        public FusebillDraftInvoiceResp GetInvoiceForPlan(FusebillInvoiceReq FusebillInvoiceReqObj)
        {
            FusebillDraftInvoiceResp fusebillDraftInvoiceResp = new FusebillDraftInvoiceResp();
            FusebillInvoiceResponse fusebillInvoiceObj = new FusebillInvoiceResponse();
            List<FusebillInvoiceResponse> lstFusebillInvoiceResponseObj = new List<FusebillInvoiceResponse>();

            FusebillErrorMessage fusebillErrorMessage = new FusebillErrorMessage();

            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                //Sql Connection to pass request and fetch response  
                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerGetFuseBillKeyValueProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@KeyName", SqlDbType.NVarChar).Value = "'APIKey', 'SubscriptionsPreviewUrl'";
                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            var dataRowApiKey = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "APIKey").FirstOrDefault();
                            string apiKey = Convert.ToString(dataRowApiKey["Value"]);

                            var dataRowSubscriptionsPreviewUrl = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "SubscriptionsPreviewUrl").FirstOrDefault();
                            string subscriptionsPreviewUrl = Convert.ToString(dataRowSubscriptionsPreviewUrl["Value"]);

                            List<string> invalidCouponCodeList = new List<string>();
                            List<string> validCouponCodeList = new List<string>();
                            List<ErrorMessage> lstErrorMessages = new List<ErrorMessage>();

                            foreach (var item in FusebillInvoiceReqObj.fusebillInvoiceRequest)
                            {
                                lstErrorMessages = ValidateCouponInvoice(item, invalidCouponCodeList, lstErrorMessages, validCouponCodeList);

                                string jsonData1 = JsonConvert.SerializeObject(item);
                                WebRequest request1 = WebRequest.Create(subscriptionsPreviewUrl);
                                request1.ContentType = "application/json";
                                request1.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                                request1.Method = "POST";
                                using (var streamWriter = new StreamWriter(request1.GetRequestStream()))
                                {
                                    streamWriter.Write(jsonData1);
                                    streamWriter.Flush();
                                    streamWriter.Close();
                                }

                                var httpResponse1 = (HttpWebResponse)request1.GetResponse();

                                var result1 = "";
                                using (var streamReader = new StreamReader(httpResponse1.GetResponseStream()))
                                {
                                    result1 = streamReader.ReadToEnd();
                                    fusebillInvoiceObj = JsonConvert.DeserializeObject<FusebillInvoiceResponse>(result1);
                                    lstFusebillInvoiceResponseObj.Add(fusebillInvoiceObj);
                                }
                            }

                            FusebillDraftInvoiceResponse fusebillDraftInvoiceResponse = new FusebillDraftInvoiceResponse();
                            List<FusebillDraftInvoiceResponse> lstFusebillDraftInvoiceResponse = new List<FusebillDraftInvoiceResponse>();
                            foreach (var item in lstFusebillInvoiceResponseObj)
                            {
                                foreach (var item1 in item.sideEffects.draftInvoice.draftCharges)
                                {
                                    fusebillDraftInvoiceResponse = new FusebillDraftInvoiceResponse();

                                    fusebillDraftInvoiceResponse.planCode = item.planCode;
                                    fusebillDraftInvoiceResponse.planName = item.planName;
                                    fusebillDraftInvoiceResponse.planDescription = item.planDescription;

                                    fusebillDraftInvoiceResponse.productId = item1.productId;
                                    fusebillDraftInvoiceResponse.productName = item1.name;
                                    fusebillDraftInvoiceResponse.productDescription = item1.description;

                                    fusebillDraftInvoiceResponse.quantity = item1.quantity;
                                    fusebillDraftInvoiceResponse.unitPrice = item1.unitPrice;
                                    fusebillDraftInvoiceResponse.amount = item1.amount;

                                    fusebillDraftInvoiceResponse.taxableAmount = item1.taxableAmount;

                                    List<Discounts> lstDiscounts = new List<Discounts>();

                                    if (item1.draftDiscounts != null)
                                    {
                                        foreach (var item2 in item1.draftDiscounts)
                                        {
                                            Discounts objDiscounts = new Discounts();
                                            objDiscounts.configuredDiscountAmount = item2.configuredDiscountAmount;
                                            objDiscounts.discountAmount = item2.amount;
                                            objDiscounts.discountType = item2.discountType;
                                            objDiscounts.discountDescription = item2.description;
                                            lstDiscounts.Add(objDiscounts);
                                        }
                                    }
                                    fusebillDraftInvoiceResponse.discounts = lstDiscounts;
                                    lstFusebillDraftInvoiceResponse.Add(fusebillDraftInvoiceResponse);
                                }

                                fusebillDraftInvoiceResp.subtotal += item.sideEffects.draftInvoice.subtotal;
                                fusebillDraftInvoiceResp.totalDiscount += item.sideEffects.draftInvoice.totalDiscount;
                                fusebillDraftInvoiceResp.total += item.sideEffects.draftInvoice.total;
                                fusebillDraftInvoiceResp.taxes = item.sideEffects.draftInvoice.tax;
                            }
                            fusebillDraftInvoiceResp.fusebillDraftInvoiceResponse = lstFusebillDraftInvoiceResponse;

                            if (lstErrorMessages != null && lstErrorMessages.Count > 0)
                                fusebillErrorMessage.errorMessage = lstErrorMessages;

                            fusebillDraftInvoiceResp.fusebillErrorMessage = fusebillErrorMessage;
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "GetInvoiceForPlan", ex.Message);
            }
            return fusebillDraftInvoiceResp;
        }


        [EnableCors("customPolicy")]
        [Route("CreateActivateSubscription")]
        [HttpPost]
        public FusebillSubscriptionResp CreateActivateSubscription()
        {
            int companyId = 0;
            if (!String.IsNullOrEmpty(Request.Headers["CompanyId"]))
                companyId = Convert.ToInt32(Request.Headers["CompanyId"]);

            string strHost = Configuration.GetSection("SMTP")["SmtpServer"];
            int port = Convert.ToInt32(Configuration.GetSection("SMTP")["SmtpPort"]);
            string strFromEmail = Configuration.GetSection("SMTP")["FromEmail"];
            string strEmailPassword = Configuration.GetSection("SMTP")["EmailPassword"];

            FusebillSubscriptionResp fusebillSubscriptionResp = new FusebillSubscriptionResp();
            try
            {
                var map = new Dictionary<string, string>();
                string strHtmlContent = string.Empty;
                string strLoginUrl = Configuration.GetSection("StoreflixServerURL")["DefaultURL"];
                string azureAccountName = Configuration.GetSection("Azure")["AccountName"];
                string azureAccountKey = Configuration.GetSection("Azure")["AccountKey"];

                int customerId = Convert.ToInt32(HttpContext.Request.Form["FusebillId"]);
                string strFranchiseCode = Convert.ToString(HttpContext.Request.Form["FranchiseCode"]);
                double paymentAmount = Convert.ToDouble(HttpContext.Request.Form["PaymentAmount"]);
                CustomerRegistrationRequest customerRegistrationRequest = new CustomerRegistrationRequest();
                List<FusebillSubscriptionRequest> fusebillSubscriptionRequest = new List<FusebillSubscriptionRequest>();
                if (!String.IsNullOrEmpty(HttpContext.Request.Form["CustomerRegistrationRequest"]))
                {
                    var jsonObjectCustomerRegistrationRequest = HttpContext.Request.Form["CustomerRegistrationRequest"].ToString();
                    customerRegistrationRequest = JsonConvert.DeserializeObject<CustomerRegistrationRequest>(jsonObjectCustomerRegistrationRequest);

                    foreach(var store in customerRegistrationRequest.RegisterStore)
                    {
                        if (string.IsNullOrWhiteSpace(store.TimeZone))
                            store.TimeZone = "Eastern Standard Time";

                        var tz = TimeZoneInfo.FindSystemTimeZoneById(store.TimeZone);
                        store.UTCOffSet = Convert.ToInt32(tz.GetUtcOffset(DateTime.Now).TotalMinutes);
                    }
                }
                if (!String.IsNullOrEmpty(HttpContext.Request.Form["FusebillSubscriptionRequest"]))
                {
                    var jsonObjectFusebillSubscriptionRequest = HttpContext.Request.Form["FusebillSubscriptionRequest"].ToString();
                    fusebillSubscriptionRequest = JsonConvert.DeserializeObject<List<FusebillSubscriptionRequest>>(jsonObjectFusebillSubscriptionRequest);
                }

                string strPassword = string.Empty;
                if (customerRegistrationRequest != null && customerRegistrationRequest.RegisterUser != null)
                    strPassword = customerRegistrationRequest.RegisterUser[0].Password;

                FusebillCustomerDetail fusebillCustomerDetail = ValidateCustomer(customerId);
                
                // Logic to by bypass $0 amount
                //int PaymentActivityId = 0;
                //if (fusebillCustomerDetail.PaymentActivityId == 0)
                //{
                //    //Call Payment Activities api
                //    PaymentActivityId = ValidatePaymentActivities(customerId);
                //}

                if (fusebillCustomerDetail.PaymentActivityId > 0 || (paymentAmount <= 0 && fusebillCustomerDetail.PaymentMethodExist))
                {
                    FusebillSubscriptionReq fusebillSubscriptionReq = new FusebillSubscriptionReq();
                    fusebillSubscriptionReq.customerId = customerId;
                    fusebillSubscriptionReq.FranchiseCode = strFranchiseCode;
                    fusebillSubscriptionReq.paymentActivityId = fusebillCustomerDetail.PaymentActivityId;
                    fusebillSubscriptionReq.customerRegistrationRequest = customerRegistrationRequest;
                    fusebillSubscriptionReq.fusebillSubscriptionRequest = fusebillSubscriptionRequest;

                    if ((fusebillCustomerDetail.EventType != null && fusebillCustomerDetail.EventType.ToLower() == "PaymentCreated".ToLower())
                        || (paymentAmount <= 0 && fusebillCustomerDetail.PaymentMethodExist))
                    {
                        if (!fusebillCustomerDetail.IsCustomerExists)
                        {
                            // Create and Update Customer in StoreLynk database
                            if (companyId > 0)
                                CustomerUpdate(companyId, customerId);
                            else
                                CreateActiveCustomer(fusebillSubscriptionReq);
                            // Create draft subscription, Activate Customer and subscripptions
                            CreateSubscription(fusebillSubscriptionReq, fusebillSubscriptionResp);
                            // Transaction successfully
                            fusebillSubscriptionResp.status = HttpStatusCode.OK;
                            fusebillSubscriptionResp.message = "Customer and Subscription created successfully.";

                            // Customer Registration email notification
                            if (companyId == 0)
                            {
                                try
                                {
                                    map.Add("{{FirstName LastName}}", customerRegistrationRequest.RegisterUser[0].FirstName + ' ' + customerRegistrationRequest.RegisterUser[0].LastName);
                                    map.Add("{Username}", customerRegistrationRequest.RegisterUser[0].Name);
                                    map.Add("{Password}", strPassword);
                                    map.Add("{URL}", strLoginUrl);

                                    strHtmlContent = Common.ReadAzureHtmlFile(azureAccountName, azureAccountKey, "RegistrationSuccess.html", map);
                                    Common.Send(strHost, port, strFromEmail, customerRegistrationRequest.RegisterUser[0].Email, "Registration Success", strHtmlContent, strEmailPassword);
                                }
                                catch (Exception ex)
                                {
                                    Common.WriteLog("CustomerController", "Customer Registration Email Notification Failed", ex.Message);
                                }
                            }
                        }
                        else
                        {
                            // Algorithum for Edit flow - FullbillId
                            // Mapped PlanProductUniqueId, FusebillId of Request with datatable table - Fetch Quantity from Request and SubscriptionProductId from datatable  
                            DataSet ds = GetSubscriptionsDataSet(customerId);
                            if (ds.Tables.Count > 0)
                            {
                                DataRow[] drActiveSubscriptions = ds.Tables[1].Select("Status NOT IN ('Cancelled')");
                                List<CustomerSubscriptions> customerSubscriptions = new List<CustomerSubscriptions>();
                                if (drActiveSubscriptions.Length > 0)
                                {
                                    customerSubscriptions = Common.ConvertDataTable<CustomerSubscriptions>(drActiveSubscriptions.CopyToDataTable());
                                }
                                foreach (FusebillSubscriptionRequest objFBSReq in fusebillSubscriptionRequest)
                                {
                                    bool isAddition = false;
                                    foreach (SubscriptionProducts objSPs in objFBSReq.subscriptionProducts)
                                    {
                                        int planProductUniqueId = objSPs.planProductUniqueId;
                                        double quantity = objSPs.quantity;
                                        var subscriptionProductId = customerSubscriptions.Where(t => t.PlanProductUniqueId == planProductUniqueId).Select(n => n.SubscriptionProductId).FirstOrDefault();
                                        if (subscriptionProductId > 0)
                                            EditSubscriptionProduct(subscriptionProductId, quantity);
                                        else
                                        {
                                            isAddition = true;
                                            break;
                                        }
                                    }
                                    if (isAddition)
                                        CreateNewSubscription(objFBSReq);
                                }
                                // Transaction successfully
                                fusebillSubscriptionResp.status = HttpStatusCode.OK;
                                fusebillSubscriptionResp.message = "Subscription updated successfully.";
                            }
                        }

                        // TODO Refund logic
                        //Commented out Refund logic - Ram 06/10/2022
                        //if (fusebillCustomerDetail.EventType.ToLower() == "PaymentCreated".ToLower() && Convert.ToDouble(fusebillCustomerDetail.Amount) == 0.1 && paymentAmount == 0.0)
                        //    RefundAmount(fusebillCustomerDetail);
                    }
                    else if (fusebillCustomerDetail.EventType != null && fusebillCustomerDetail.EventType.ToLower() == "PaymentFailed".ToLower())
                    {
                        //Update Customer Json data into SL_FusebillPayment
                        UpdateCustomerJson(fusebillSubscriptionReq);
                        // Payment failed 
                        fusebillSubscriptionResp.status = HttpStatusCode.BadRequest;
                        fusebillSubscriptionResp.message = "Payment failed.";
                    }
                }
                else
                {
                    fusebillSubscriptionResp.status = HttpStatusCode.BadRequest;
                    fusebillSubscriptionResp.message = "Payment not found";
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                if (ex.Message.Contains("429"))
                {
                    fusebillSubscriptionResp.message = "API Rate limit has been exceeded.";
                    fusebillSubscriptionResp.status = HttpStatusCode.InternalServerError;
                }
                else
                {
                    fusebillSubscriptionResp.message = "Internal server error";
                    fusebillSubscriptionResp.status = HttpStatusCode.InternalServerError;
                }
                Common.WriteLog("CustomerController", "CreateActivateSubscription", ex.Message);
            }
            return fusebillSubscriptionResp;
        }

        [EnableCors("customPolicy")]
        [Route("ViewSubscriptions")]
        [HttpGet]
        public CustomerSubscriptionDetailResponse ViewSubscriptions(int CustomerId)
        {
            CustomerSubscriptionDetailResponse customerSubscriptionDetailResponse = new CustomerSubscriptionDetailResponse();
            List<CustomerSubscriptions> customerSubscriptions = new List<CustomerSubscriptions>();
            try
            {
                DataSet ds = GetSubscriptionsDataSet(CustomerId);
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[3].Rows.Count > 0)
                        customerSubscriptionDetailResponse.FranchaiseCode = Convert.ToString(ds.Tables[3].Rows[0]["FranchiseCode"]);
                    // Fetch active subscriptions
                    DataRow[] drActiveSubscriptions = ds.Tables[1].Select("Status='Active'");
                    if (drActiveSubscriptions.Length > 0)
                    {
                        customerSubscriptions = Common.ConvertDataTable<CustomerSubscriptions>(drActiveSubscriptions.CopyToDataTable());
                    }
                    foreach (var objSubs in customerSubscriptions)
                    {
                        objSubs.couponDetail = new List<CouponDetail>();
                        DataRow[] drCouponDetails = ds.Tables[2].Select("PlanProductUniqueId=" + objSubs.PlanProductUniqueId);
                        if (drCouponDetails.Length > 0)
                            objSubs.couponDetail = Common.ConvertDataTable<CouponDetail>(drCouponDetails.CopyToDataTable());
                    }
                }
                customerSubscriptionDetailResponse.customerSubscriptions = customerSubscriptions;
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "ViewSubscriptions", ex.Message);
            }

            return customerSubscriptionDetailResponse;
        }

        [EnableCors("customPolicy")]
        [Route("ManageSubscriptions")]
        [HttpGet]
        public FusebillSSOUrl ManageSubscriptions(int CustomerId)
        {
            string strSSOUrl = string.Empty;
            FusebillSSOUrl ObjFusebillSSOUrl = new FusebillSSOUrl();
            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                //Sql Connection to pass request and fetch response  
                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerGetFuseBillKeyValueProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@KeyName", SqlDbType.NVarChar).Value = "'APIKey', 'SSOToken', 'SSOLogin'";

                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            var dataRowApiKey = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "APIKey").FirstOrDefault();
                            string apiKey = Convert.ToString(dataRowApiKey["Value"]);

                            var dataRowSSOToken = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "SSOToken").FirstOrDefault();
                            string ssoTokenUrl = Convert.ToString(dataRowSSOToken["Value"]);
                            ssoTokenUrl = ssoTokenUrl.Replace("{customerId}", CustomerId.ToString());

                            var dataRowSSOLogin = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "SSOLogin").FirstOrDefault();
                            string ssoLoginUrl = Convert.ToString(dataRowSSOLogin["Value"]);

                            //Configure URI
                            WebRequest request = WebRequest.Create(ssoTokenUrl);
                            //Add Content type
                            request.ContentType = "application/json";
                            //Add Api key authorization
                            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                            //Set request method
                            request.Method = "GET";
                            //Perform the request
                            var httpResponse = (HttpWebResponse)request.GetResponse();
                            //Record the response from our request
                            var result = "";
                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                result = streamReader.ReadToEnd();
                                //As result is coming with "\ /" so we are taking the substring 
                                strSSOUrl = ssoLoginUrl + result.Substring(1, result.Length - 2);
                                ObjFusebillSSOUrl.SSOUrl = strSSOUrl;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "ManageSubscriptions", ex.Message);
            }
            return ObjFusebillSSOUrl;
        }

        private DataSet GetSubscriptionsDataSet(int fusebillId)
        {
            DataSet dsSubscriptions = new DataSet();
            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerSubscriptionsProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@FusebillId", SqlDbType.Int).Value = fusebillId;
                    connection.Open();

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        //Moving the SQL response into the DataTable
                        sda.Fill(dsSubscriptions);
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "GetSubscriptionsDataSet", ex.Message);
            }
            return dsSubscriptions;
        }

        private void EditSubscriptionProduct(int subscriptionProductId, double deltaQuantity)
        {
            FusebillErrorMessage fusebillErrorMessage = new FusebillErrorMessage();
            List<ErrorMessage> lstErrorMessages = new List<ErrorMessage>();
            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerGetFuseBillKeyValueProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@KeyName", SqlDbType.NVarChar).Value = "'APIKey', 'SubscriptionProductUrl'";
                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            var dataRowApiKey = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "APIKey").FirstOrDefault();
                            string apiKey = Convert.ToString(dataRowApiKey["Value"]);

                            var dataRowSubscriptionProductUrl = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "SubscriptionProductUrl").FirstOrDefault();
                            string subscriptionProductUrl = Convert.ToString(dataRowSubscriptionProductUrl["Value"]);

                            //Json data, with subscription product Id with delta quantity
                            string jsonData = "{'subscriptionProductId':" + subscriptionProductId + ", 'deltaQuantity': " + deltaQuantity + "}";

                            WebRequest request = WebRequest.Create(subscriptionProductUrl);
                            request.ContentType = "application/json";
                            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                            request.Method = "POST";
                            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                            {
                                streamWriter.Write(jsonData);
                                streamWriter.Flush();
                                streamWriter.Close();
                            }
                            //Perform the request 
                            var httpResponse = (HttpWebResponse)request.GetResponse();
                            //Record the response from our request 
                            var result = "";
                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                result = streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "EditSubscription", ex.Message);
                throw;
            }
        }

        private void CreateNewSubscription(FusebillSubscriptionRequest objFBSReq)
        {
            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerGetFuseBillKeyValueProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@KeyName", SqlDbType.NVarChar).Value = "'APIKey', 'SubscriptionsCreateUrl', 'SubscriptionsActivationUrl'";
                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            var dataRowApiKey = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "APIKey").FirstOrDefault();
                            string apiKey = Convert.ToString(dataRowApiKey["Value"]);

                            var dataRowSubscriptionsCreateUrl = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "SubscriptionsCreateUrl").FirstOrDefault();
                            string subscriptionsCreateUrl = Convert.ToString(dataRowSubscriptionsCreateUrl["Value"]);

                            List<string> subscriptionIds = new List<string>();

                            ValidateCouponSubscription(objFBSReq);
                            SaveCouponDetails(objFBSReq.customerId, 0, objFBSReq.couponCodes);

                            string jsonData = JsonConvert.SerializeObject(objFBSReq);
                            WebRequest request = WebRequest.Create(subscriptionsCreateUrl);
                            request.ContentType = "application/json";
                            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                            request.Method = "POST";
                            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                            {
                                streamWriter.Write(jsonData);
                                streamWriter.Flush();
                                streamWriter.Close();
                            }

                            var httpResponse = (HttpWebResponse)request.GetResponse();
                            //Record the response from our request
                            var result = "";
                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                result = streamReader.ReadToEnd();
                                JsonDocument doc = JsonDocument.Parse(result);
                                JsonElement root = doc.RootElement;
                                JsonElement jecustomerId = root.GetProperty("customerId");
                                JsonElement jeplanRevisionId = root.GetProperty("planFrequency").GetProperty("planRevisionId");
                                subscriptionIds.Add(jeplanRevisionId.ToString());
                            }

                            // TO DO - commenting this as it will activate subscription in fusebill unnecessary 
                            ActivateSubscription(subscriptionIds);
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "CreateSubscription", ex.Message);
                throw;
            }
        }

        private void ActivateCustomer(int CustomerId)
        {
            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.SL_RegisterCustomerActivateProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@FusebillId", SqlDbType.Int).Value = CustomerId;
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "ActivateCustomer", ex.Message);
                throw ex;
            }
        }

        private void UpdateCustomerJson(FusebillSubscriptionReq fusebillSubscriptionReq)
        {
            try
            {
                string jsonDataCustomerDetails = JsonConvert.SerializeObject(fusebillSubscriptionReq.customerRegistrationRequest.RegisterCompany[0]);

                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                //Sql Connection to pass request and fetch response  
                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerUpdateJsonProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@FusebillId", SqlDbType.Int).Value = fusebillSubscriptionReq.customerId;
                    cmd.Parameters.Add("@PaymentActivityId", SqlDbType.Int).Value = fusebillSubscriptionReq.paymentActivityId;
                    cmd.Parameters.Add("@JsonText", SqlDbType.NVarChar).Value = jsonDataCustomerDetails;

                    connection.Open();
                    using (SqlDataReader dbReader = cmd.ExecuteReader())
                    {
                        while (dbReader.Read())
                        {
                            string strResult = Convert.ToString(dbReader["SPResult"]);
                            if (strResult == "0")
                                HttpContext.Response.StatusCode = 500;
                            else
                                HttpContext.Response.StatusCode = 200;
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "UpdateCustomerJson", ex.Message);
            }
        }

        private void CreateActiveCustomer(FusebillSubscriptionReq fusebillSubscriptionReq)
        {
            try
            {
                if (fusebillSubscriptionReq.customerId > 0)
                {
                    int UserId = 0;
                    if (!String.IsNullOrEmpty(Request.Headers["UserId"]))
                        UserId = Convert.ToInt32(Request.Headers["UserId"]);

                    string ImagePath = string.Empty;
                    string FileNameToUpload = string.Empty;
                    if (HttpContext.Request.Form.Files.Count() > 0)
                    {
                        var companyFile = HttpContext.Request.Form.Files[0];
                        string GUID = System.Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(companyFile.FileName);
                        FileNameToUpload = GUID + "_" + Path.GetFileNameWithoutExtension(companyFile.FileName) + extension;
                        ImagePath = UpLoadCompanyLogo(FileNameToUpload);
                        fusebillSubscriptionReq.customerRegistrationRequest.RegisterCompany[0].LogoFile = ImagePath;
                    }


                    //Getting the connection string from appsettings.json which is included in Starup.cs
                    string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                    using (SqlConnection connection = new SqlConnection(strSqlConnection))
                    {
                        //Calling the stored procedure with all the required paramaters 
                        SqlCommand cmd = new SqlCommand("PT_SL_RegisterCustomer", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 300;

                        fusebillSubscriptionReq.customerRegistrationRequest.RegisterUser.ForEach(s => s.Password = SFF_ENCRYPT(s.Password));
                        DataTable dtCompanyDetails = Common.ToDataTable(fusebillSubscriptionReq.customerRegistrationRequest.RegisterCompany);
                        DataTable dtStoreDetails = Common.ToDataTable(fusebillSubscriptionReq.customerRegistrationRequest.RegisterStore);
                        DataTable dtUserDetails = Common.ToDataTable(fusebillSubscriptionReq.customerRegistrationRequest.RegisterUser);
                        DataTable dtSubsciptionDetails = Common.ToDataTable(fusebillSubscriptionReq.customerRegistrationRequest.RegisterSubscription);

                        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
                        cmd.Parameters.Add("@FusebillId", SqlDbType.Int).Value = fusebillSubscriptionReq.customerId;
                        cmd.Parameters.Add("@FranchiseCode", SqlDbType.NVarChar).Value = fusebillSubscriptionReq.FranchiseCode;

                        var registerCompany = new SqlParameter("@CompanyDetails", SqlDbType.Structured);
                        registerCompany.TypeName = "dbo.[CompanyDetails]";
                        registerCompany.Value = dtCompanyDetails;
                        cmd.Parameters.Add(registerCompany);


                        var registerStore = new SqlParameter("@StoreDetails", SqlDbType.Structured);
                        registerStore.TypeName = "dbo.[StoreDetails]";
                        registerStore.Value = dtStoreDetails;
                        cmd.Parameters.Add(registerStore);


                        var registerUser = new SqlParameter("@UserDetails", SqlDbType.Structured);
                        registerUser.TypeName = "dbo.[UserDetails]";
                        registerUser.Value = dtUserDetails;
                        cmd.Parameters.Add(registerUser);


                        var registerSubscriptions = new SqlParameter("@CustomerSubscriptions", SqlDbType.Structured);
                        registerSubscriptions.TypeName = "dbo.[CustomerSubscriptions]";
                        registerSubscriptions.Value = dtSubsciptionDetails;
                        cmd.Parameters.Add(registerSubscriptions);


                        connection.Open();
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            using (DataSet ds = new DataSet())
                            {
                                //Moving the SQL response into the DataTable
                                sda.Fill(ds);

                                DataTable dtResult = ds.Tables[0];
                                string SPResult = dtResult.Rows[0]["SPResult"].ToString();
                                if (SPResult == "0")
                                    HttpContext.Response.StatusCode = 500;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "CreateActiveCustomer", ex.Message);
                throw;
            }
        }

        private string UpLoadCompanyLogo(string FileNameToUpload)
        {
            string strUrl = string.Empty;
            try
            {
                var companyLogo = HttpContext.Request.Form.Files[0];
                string azureAccountName = Configuration.GetSection("Azure")["AccountName"];
                string azureAccountKey = Configuration.GetSection("Azure")["AccountKey"];

                string fname = "Company_Images/" + FileNameToUpload;

                StorageSharedKeyCredential storageCredentials = new StorageSharedKeyCredential(azureAccountName, azureAccountKey);
                Uri blobUri = new Uri("https://" + azureAccountName + ".blob.core.windows.net/" + "storelynk" + "/" + fname);
                BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

                strUrl = "https://" + azureAccountName + ".blob.core.windows.net/" + "storelynk" + "/" + fname;
                using (var stream = new MemoryStream())
                {
                    companyLogo.CopyTo(stream);
                    stream.Position = 0;
                    blobClient.Upload(stream);
                }
            }
            catch (Exception ex)
            {
                Common.WriteLog("CustomerController", "UpLoadCompanyLogo", ex.Message);
            }
            return strUrl;
        }

        private void CreateSubscription(FusebillSubscriptionReq fusebillSubscriptionReq, FusebillSubscriptionResp fusebillSubscriptionResp)
        {
            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerGetFuseBillKeyValueProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@KeyName", SqlDbType.NVarChar).Value = "'APIKey', 'SubscriptionsCreateUrl', 'SubscriptionsActivationUrl'";
                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            var dataRowApiKey = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "APIKey").FirstOrDefault();
                            string apiKey = Convert.ToString(dataRowApiKey["Value"]);

                            var dataRowSubscriptionsCreateUrl = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "SubscriptionsCreateUrl").FirstOrDefault();
                            string subscriptionsCreateUrl = Convert.ToString(dataRowSubscriptionsCreateUrl["Value"]);

                            List<string> subscriptionIds = new List<string>();

                            foreach (var item in fusebillSubscriptionReq.fusebillSubscriptionRequest)
                            {
                                //  TO DO comment if not required to reduce calls9
                                //ValidateCouponSubscription(item);
                                //SaveCouponDetails(item.customerId, item.planId, item.couponCodes);

                                string jsonData = JsonConvert.SerializeObject(item);
                                WebRequest request = WebRequest.Create(subscriptionsCreateUrl);
                                request.ContentType = "application/json";
                                request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                                request.Method = "POST";
                                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                                {
                                    streamWriter.Write(jsonData);
                                    streamWriter.Flush();
                                    streamWriter.Close();
                                }

                                var httpResponse = (HttpWebResponse)request.GetResponse();
                                //Record the response from our request
                                var result = "";
                                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    result = streamReader.ReadToEnd();
                                    JsonDocument doc = JsonDocument.Parse(result);
                                    JsonElement root = doc.RootElement;
                                    JsonElement jecustomerId = root.GetProperty("customerId");
                                    JsonElement jeplanRevisionId = root.GetProperty("planFrequency").GetProperty("planRevisionId");
                                    subscriptionIds.Add(jeplanRevisionId.ToString());
                                }
                            }

                            // TO DO - Activate Customer in Fusebill system - Modify key in database for Activation
                            ActivateCustomerInFusebill(fusebillSubscriptionReq.customerId);

                            // TO DO - commenting this as it will activate subscription in fusebill unnecessary 
                            ActivateSubscription(subscriptionIds);
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "CreateSubscription", ex.Message);
                throw;
            }
        }

        private void ActivateCustomerInFusebill(int CustomerId)
        {
            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerGetFuseBillKeyValueProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@KeyName", SqlDbType.NVarChar).Value = "'APIKey', 'CustomerActivationUrl'";
                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            var dataRowApiKey = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "APIKey").FirstOrDefault();
                            string apiKey = Convert.ToString(dataRowApiKey["Value"]);

                            var dataRowCustomerActivationUrl = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "CustomerActivationUrl").FirstOrDefault();
                            string CustomerActivationUrl = Convert.ToString(dataRowCustomerActivationUrl["Value"]);

                            //Json Payload
                            //string jsonDataCustomer = "{'customerId':" + CustomerId+ ",'activateAllSubscriptions': true,'temporarilyDisableAutoPost': true}";
                            string jsonDataCustomer = "{'customerId':" + CustomerId + ",'activateAllSubscriptions': false,'temporarilyDisableAutoPost': false}";
                            WebRequest request = WebRequest.Create(CustomerActivationUrl);
                            request.ContentType = "application/json";
                            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                            request.Method = "POST";

                            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                            {
                                streamWriter.Write(jsonDataCustomer);
                                streamWriter.Flush();
                                streamWriter.Close();
                            }
                            //Perform the request
                            var httpResponse = (HttpWebResponse)request.GetResponse();
                            //Record the response from our request
                            var resultCustomer = "";
                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                resultCustomer = streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "ActivateCustomerFusebill", ex.Message);
                throw;
            }
        }

        private void ActivateSubscription(List<string> subscriptionIds)
        {
            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerGetFuseBillKeyValueProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@KeyName", SqlDbType.NVarChar).Value = "'APIKey', 'SubscriptionsActivationUrl'";
                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            var dataRowApiKey = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "APIKey").FirstOrDefault();
                            string apiKey = Convert.ToString(dataRowApiKey["Value"]);

                            var dataRowSubscriptionsActivationUrl = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "SubscriptionsActivationUrl").FirstOrDefault();
                            string subscriptionsActivationUrl = Convert.ToString(dataRowSubscriptionsActivationUrl["Value"]);

                            string jsonDatasubscriptionIds = JsonConvert.SerializeObject(subscriptionIds);
                            //Json data, with Subscription IDs that match the subscriptions to activate 
                            string jsonDataActivate = "{'subscriptionIds':" + jsonDatasubscriptionIds + ",'invoicePreview':false,'temporarilyDisableAutoPost':false}";
                            WebRequest requestActivate = WebRequest.Create(subscriptionsActivationUrl);
                            requestActivate.ContentType = "application/json";
                            requestActivate.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                            requestActivate.Method = "POST";
                            using (var streamWriter = new StreamWriter(requestActivate.GetRequestStream()))
                            {
                                streamWriter.Write(jsonDataActivate);
                                streamWriter.Flush();
                                streamWriter.Close();
                            }

                            var httpResponseActivate = (HttpWebResponse)requestActivate.GetResponse();
                            //Record the response from our request 
                            var resultActivate = "";
                            using (var streamReader = new StreamReader(httpResponseActivate.GetResponseStream()))
                            {
                                resultActivate = streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "ActivateSubscription", ex.Message);
                throw;
            }
        }

        private void ValidateCouponSubscription(FusebillSubscriptionRequest item)
        {

            List<ErrorMessage> lstErrorMessages = new List<ErrorMessage>();
            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerGetFuseBillKeyValueProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@KeyName", SqlDbType.NVarChar).Value = "'APIKey', 'CouponsUrl'";
                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            var dataRowApiKey = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "APIKey").FirstOrDefault();
                            string apiKey = Convert.ToString(dataRowApiKey["Value"]);

                            var dataRowCouponsUrl = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "CouponsUrl").FirstOrDefault();
                            string couponsUrl = Convert.ToString(dataRowCouponsUrl["Value"]);

                            FusebillCouponRequest fusebillCouponRequestObj = new FusebillCouponRequest();
                            FusebillCouponResponse fusebillCouponResponseObj = new FusebillCouponResponse();

                            List<string> couponCodeList = new List<string>();

                            fusebillCouponRequestObj.planId = 0;

                            if (item.couponCodes != null && item.couponCodes.Count >= 1)
                            {
                                foreach (var itemCoupon in item.couponCodes.ToList())
                                {
                                    try
                                    {
                                        fusebillCouponRequestObj.couponCode = itemCoupon.ToString();
                                        string jsonDataCoupon = JsonConvert.SerializeObject(fusebillCouponRequestObj);
                                        WebRequest requestCoupon = WebRequest.Create(couponsUrl);
                                        requestCoupon.ContentType = "application/json";
                                        requestCoupon.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                                        requestCoupon.Method = "POST";
                                        using (var streamWriter = new StreamWriter(requestCoupon.GetRequestStream()))
                                        {
                                            streamWriter.Write(jsonDataCoupon);
                                            streamWriter.Flush();
                                            streamWriter.Close();
                                        }

                                        var httpResponseCoupon = (HttpWebResponse)requestCoupon.GetResponse();
                                        var resultCoupon = "";
                                        using (var streamReader = new StreamReader(httpResponseCoupon.GetResponseStream()))
                                        {
                                            resultCoupon = streamReader.ReadToEnd();
                                            fusebillCouponResponseObj = JsonConvert.DeserializeObject<FusebillCouponResponse>(resultCoupon);
                                        }

                                        if (fusebillCouponResponseObj.valid)
                                        {
                                            couponCodeList.Add(fusebillCouponRequestObj.couponCode);
                                        }
                                    }
                                    catch (Exception ex) //Logging the error into file while exception
                                    {
                                        Common.WriteLog("CustomerController", "ValidateCouponSubscription", ex.Message);
                                    }
                                    item.couponCodes.Clear();
                                    item.couponCodes.AddRange(couponCodeList);
                                }
                            }
                            else
                            {
                                fusebillCouponRequestObj.couponCode = "";
                            }
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "ValidateCouponSubscription", ex.Message);
            }
        }

        private void SaveCouponDetails(int customerId, int planId, List<string> couponCodes)
        {
            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerGetFuseBillKeyValueProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@KeyName", SqlDbType.NVarChar).Value = "'APIKey', 'CouponCodeUrl', 'CouponDetailUrl'";
                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            var dataRowApiKey = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "APIKey").FirstOrDefault();
                            string apiKey = Convert.ToString(dataRowApiKey["Value"]);

                            var dataRowCouponCodeUrl = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "CouponCodeUrl").FirstOrDefault();
                            string couponCodeUrl = Convert.ToString(dataRowCouponCodeUrl["Value"]);

                            foreach (var itemCoupon in couponCodes)
                            {
                                var dataRowCouponDetailUrl = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "CouponDetailUrl").FirstOrDefault();
                                string couponDetailUrl = Convert.ToString(dataRowCouponDetailUrl["Value"]);
                                JsonElement jeCouponId = new JsonElement();
                                JsonElement jeName = new JsonElement();
                                JsonElement jeDescription = new JsonElement();
                                JsonElement jeEligibilityStartDate = new JsonElement();
                                JsonElement jeEligibilityEndDate = new JsonElement();
                                JsonElement jeStatus = new JsonElement();
                                JsonElement jePlanProductUniqueId = new JsonElement();

                                string couponCodeUrl1 = couponCodeUrl.Replace("{CouponCode}", itemCoupon);
                                WebRequest request = WebRequest.Create(couponCodeUrl1);
                                //Add Content type
                                request.ContentType = "application/json";
                                request.ContentType = "application/json";
                                request.ContentLength = 0;
                                //Add Api key authorization
                                request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                                //Set request method
                                request.Method = "GET";
                                //Perform the request
                                var httpResponse = (HttpWebResponse)request.GetResponse();
                                //Record the response from our request
                                var result = "";
                                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    result = streamReader.ReadToEnd();
                                    JsonDocument doc = JsonDocument.Parse(result);
                                    JsonElement root = doc.RootElement;

                                    for (int i = 0; i < root.GetArrayLength(); i++)
                                    {
                                        var u1 = root[i];
                                        jeCouponId = u1.GetProperty("id");
                                        jeName = u1.GetProperty("name");
                                        jeDescription = u1.GetProperty("description");
                                        jeEligibilityStartDate = u1.GetProperty("eligibilityStartDate");
                                        jeEligibilityEndDate = u1.GetProperty("eligibilityEndDate");
                                        jeStatus = u1.GetProperty("status");
                                        couponDetailUrl = couponDetailUrl.Replace("{CouponId}", jeCouponId.ToString());
                                    }
                                }

                                WebRequest request1 = WebRequest.Create(couponDetailUrl);
                                //Add Content type
                                request1.ContentType = "application/json";
                                request1.ContentLength = 0;
                                //Add Api key authorization
                                request1.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                                //Set request method
                                request1.Method = "GET";
                                //Perform the request
                                var httpResponse1 = (HttpWebResponse)request1.GetResponse();
                                //Record the response from our request
                                var result1 = "";
                                using (var streamReader = new StreamReader(httpResponse1.GetResponseStream()))
                                {
                                    result1 = streamReader.ReadToEnd();
                                    JsonDocument doc1 = JsonDocument.Parse(result1);
                                    JsonElement root1 = doc1.RootElement;
                                    JsonElement jecustomerId = root1.GetProperty("id");
                                    for (int i = 0; i < root1.GetProperty("plans").GetArrayLength(); i++)
                                    {
                                        var u1 = root1.GetProperty("plans")[i];
                                        int planCouponId = root1.GetProperty("plans")[i].GetProperty("id").GetInt32();

                                        // Add coupon codes details based on plan id
                                        if (planCouponId == planId)
                                        {
                                            for (int j = 0; j < u1.GetProperty("planProducts").GetArrayLength(); j++)
                                            {
                                                jePlanProductUniqueId = u1.GetProperty("planProducts")[j].GetProperty("id");

                                                //Calling the stored procedure with all the required paramaters 
                                                SqlCommand cmd1 = new SqlCommand(Keys.FusebillSaveUpdateCouponDetailProcedure, connection);
                                                cmd1.CommandType = CommandType.StoredProcedure;
                                                cmd1.CommandTimeout = 300;

                                                cmd1.Parameters.Add("@FusebillId", SqlDbType.Int).Value = customerId;
                                                cmd1.Parameters.Add("@PlanId", SqlDbType.Int).Value = planId;
                                                cmd1.Parameters.Add("@CouponCode", SqlDbType.NVarChar).Value = itemCoupon;
                                                cmd1.Parameters.Add("@CouponId", SqlDbType.Int).Value = jeCouponId.GetInt32();
                                                cmd1.Parameters.Add("@Name", SqlDbType.NVarChar).Value = jeName.GetString();
                                                cmd1.Parameters.Add("@Description", SqlDbType.NVarChar).Value = jeDescription.GetString();

                                                if (jeEligibilityStartDate.GetString() == null)
                                                    cmd1.Parameters.Add("@EligibilityStartDate", SqlDbType.DateTime).Value = DateTime.MaxValue;
                                                else
                                                    cmd1.Parameters.Add("@EligibilityStartDate", SqlDbType.DateTime).Value = jeEligibilityStartDate.GetDateTime();

                                                if (jeEligibilityEndDate.GetString() == null)
                                                    cmd1.Parameters.Add("@EligibilityEndDate", SqlDbType.DateTime).Value = DateTime.MaxValue;
                                                else
                                                    cmd1.Parameters.Add("@EligibilityEndDate", SqlDbType.DateTime).Value = jeEligibilityEndDate.GetDateTime();

                                                cmd1.Parameters.Add("@Status", SqlDbType.NVarChar).Value = jeStatus.GetString();
                                                cmd1.Parameters.Add("@PlanProductUniqueId", SqlDbType.Int).Value = jePlanProductUniqueId.GetInt32();

                                                using (SqlDataReader dbReader = cmd1.ExecuteReader())
                                                {
                                                    while (dbReader.Read())
                                                    {
                                                        if (Convert.ToInt32(dbReader["SPResult"]) == 0)
                                                            HttpContext.Response.StatusCode = 500;
                                                        else
                                                            HttpContext.Response.StatusCode = 200;
                                                    }
                                                }
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "SaveCouponDetails", ex.Message);
                throw;
            }
        }

        private List<ErrorMessage> ValidateCouponSubscription1(FusebillSubscriptionRequest item, List<string> invalidCouponCodeList)
        {

            List<ErrorMessage> lstErrorMessages = new List<ErrorMessage>();
            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerGetFuseBillKeyValueProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@KeyName", SqlDbType.NVarChar).Value = "'APIKey', 'CouponsUrl'";
                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            var dataRowApiKey = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "APIKey").FirstOrDefault();
                            string apiKey = Convert.ToString(dataRowApiKey["Value"]);

                            var dataRowCouponsUrl = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "CouponsUrl").FirstOrDefault();
                            string couponsUrl = Convert.ToString(dataRowCouponsUrl["Value"]);

                            FusebillCouponRequest fusebillCouponRequestObj = new FusebillCouponRequest();
                            FusebillCouponResponse fusebillCouponResponseObj = new FusebillCouponResponse();

                            List<string> couponCodeList = new List<string>();

                            fusebillCouponRequestObj.planId = 0;

                            if (item.couponCodes != null && item.couponCodes.Count >= 1)
                            {
                                foreach (var itemCoupon in item.couponCodes.ToList())
                                {
                                    try
                                    {
                                        fusebillCouponRequestObj.couponCode = itemCoupon.ToString();
                                        string jsonDataCoupon = JsonConvert.SerializeObject(fusebillCouponRequestObj);
                                        WebRequest requestCoupon = WebRequest.Create(couponsUrl);
                                        requestCoupon.ContentType = "application/json";
                                        requestCoupon.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                                        requestCoupon.Method = "POST";
                                        using (var streamWriter = new StreamWriter(requestCoupon.GetRequestStream()))
                                        {
                                            streamWriter.Write(jsonDataCoupon);
                                            streamWriter.Flush();
                                            streamWriter.Close();
                                        }

                                        var httpResponseCoupon = (HttpWebResponse)requestCoupon.GetResponse();
                                        var resultCoupon = "";
                                        using (var streamReader = new StreamReader(httpResponseCoupon.GetResponseStream()))
                                        {
                                            resultCoupon = streamReader.ReadToEnd();
                                            fusebillCouponResponseObj = JsonConvert.DeserializeObject<FusebillCouponResponse>(resultCoupon);
                                        }

                                        if (fusebillCouponResponseObj.valid)
                                        {
                                            couponCodeList.Add(fusebillCouponRequestObj.couponCode);
                                        }
                                    }
                                    catch (Exception ex) //Logging the error into file while exception
                                    {
                                        if (!ex.Message.Contains("429"))
                                        {
                                            if (!invalidCouponCodeList.Contains(itemCoupon))
                                            {
                                                ErrorMessage errorMessage = new ErrorMessage();
                                                errorMessage.message = "Invalid Coupon code : " + itemCoupon;
                                                errorMessage.status = HttpStatusCode.NotFound;
                                                lstErrorMessages.Add(errorMessage);
                                                invalidCouponCodeList.Add(itemCoupon);
                                            }
                                        }
                                    }
                                    item.couponCodes.Clear();
                                    item.couponCodes.AddRange(couponCodeList);
                                }
                            }
                            else
                            {
                                fusebillCouponRequestObj.couponCode = "";
                            }
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "ValidateCouponSubscription", ex.Message);
            }
            return lstErrorMessages;
        }

        private List<ErrorMessage> ValidateCouponInvoice(FusebillInvoiceRequest item, List<string> invalidCouponCodeList, List<ErrorMessage> lstErrorMessages, List<string> validCouponCodeList)
        {
            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerGetFuseBillKeyValueProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@KeyName", SqlDbType.NVarChar).Value = "'APIKey', 'CouponsUrl'";
                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            var dataRowApiKey = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "APIKey").FirstOrDefault();
                            string apiKey = Convert.ToString(dataRowApiKey["Value"]);

                            var dataRowCouponsUrl = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "CouponsUrl").FirstOrDefault();
                            string couponsUrl = Convert.ToString(dataRowCouponsUrl["Value"]);

                            FusebillCouponRequest fusebillCouponRequestObj = new FusebillCouponRequest();
                            FusebillCouponResponse fusebillCouponResponseObj = new FusebillCouponResponse();

                            List<string> couponCodeList = new List<string>();
                            fusebillCouponRequestObj.planId = item.planId;

                            if (item.couponCodes != null && item.couponCodes.Count >= 1)
                            {
                                foreach (var itemCoupon in item.couponCodes.ToList())
                                {
                                    try
                                    {
                                        fusebillCouponRequestObj.couponCode = itemCoupon.ToString();
                                        string jsonDataCoupon = JsonConvert.SerializeObject(fusebillCouponRequestObj);
                                        WebRequest requestCoupon = WebRequest.Create(couponsUrl);
                                        requestCoupon.ContentType = "application/json";
                                        requestCoupon.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                                        requestCoupon.Method = "POST";
                                        using (var streamWriter = new StreamWriter(requestCoupon.GetRequestStream()))
                                        {
                                            streamWriter.Write(jsonDataCoupon);
                                            streamWriter.Flush();
                                            streamWriter.Close();
                                        }

                                        var httpResponseCoupon = (HttpWebResponse)requestCoupon.GetResponse();
                                        var resultCoupon = "";
                                        using (var streamReader = new StreamReader(httpResponseCoupon.GetResponseStream()))
                                        {
                                            resultCoupon = streamReader.ReadToEnd();
                                            fusebillCouponResponseObj = JsonConvert.DeserializeObject<FusebillCouponResponse>(resultCoupon);
                                        }

                                        if (fusebillCouponResponseObj.valid)
                                        {
                                            couponCodeList.Add(fusebillCouponRequestObj.couponCode);
                                            validCouponCodeList.Add(fusebillCouponRequestObj.couponCode);
                                            if (invalidCouponCodeList.Contains(fusebillCouponRequestObj.couponCode))
                                            {
                                                invalidCouponCodeList.Remove(fusebillCouponRequestObj.couponCode);
                                                var foundCoupon = lstErrorMessages.Find(x => x.message.Contains(fusebillCouponRequestObj.couponCode));
                                                if (foundCoupon != null)
                                                    lstErrorMessages.Remove(foundCoupon);
                                            }
                                        }
                                        else
                                        {
                                            if (!validCouponCodeList.Contains(fusebillCouponRequestObj.couponCode) && !invalidCouponCodeList.Contains(fusebillCouponRequestObj.couponCode))
                                            {
                                                ErrorMessage errorMessage = new ErrorMessage();
                                                errorMessage.message = "Invalid Coupon code : " + fusebillCouponRequestObj.couponCode;
                                                errorMessage.status = HttpStatusCode.NotFound;
                                                lstErrorMessages.Add(errorMessage);
                                                invalidCouponCodeList.Add(fusebillCouponRequestObj.couponCode);
                                            }
                                        }
                                    }
                                    catch (Exception ex) //Logging the error into file while exception
                                    {
                                        if (!ex.Message.Contains("429"))
                                        {
                                            if (!invalidCouponCodeList.Contains(itemCoupon))
                                            {
                                                ErrorMessage errorMessage = new ErrorMessage();
                                                errorMessage.message = "Invalid Coupon code : " + itemCoupon;
                                                errorMessage.status = HttpStatusCode.NotFound;
                                                lstErrorMessages.Add(errorMessage);
                                                invalidCouponCodeList.Add(itemCoupon);
                                            }
                                        }
                                    }
                                    item.couponCodes.Clear();
                                    item.couponCodes.AddRange(couponCodeList);
                                }
                            }
                            else
                            {
                                fusebillCouponRequestObj.couponCode = "";
                            }
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "ValidateCouponInvoice", ex.Message);
            }
            return lstErrorMessages;
        }

        private FusebillCustomerDetail ValidateCustomer(int customerId)
        {
            FusebillCustomerDetail fusebillCustomerDetail = new FusebillCustomerDetail();
            try
            {
                if (customerId > 0)
                {
                    string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                    //Sql Connection to pass request and fetch response  
                    using (SqlConnection connection = new SqlConnection(strSqlConnection))
                    {
                        //Calling the stored procedure with all the required paramaters 
                        SqlCommand cmd = new SqlCommand(Keys.SL_RegisterCustomerStatusCheckProcedure, connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 300;

                        cmd.Parameters.Add("@FusebillId", SqlDbType.Int).Value = customerId;
                        connection.Open();

                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            using (DataSet ds = new DataSet())
                            {
                                //Moving the SQL response into the DataTable
                                sda.Fill(ds);

                                DataTable dtCheckCustomerExists = ds.Tables[0];
                                fusebillCustomerDetail.CustomerId = customerId;
                                fusebillCustomerDetail.IsCustomerExists = Convert.ToBoolean(dtCheckCustomerExists.Rows[0]["IsCustomerExist"]);

                                DataTable dtCheckEventAndActivity = ds.Tables[1];
                                if (dtCheckEventAndActivity.Rows.Count > 0)
                                {
                                    fusebillCustomerDetail.PaymentActivityId = Convert.ToInt32(dtCheckEventAndActivity.Rows[0]["PaymentActivityId"]);
                                    fusebillCustomerDetail.EventType = Convert.ToString(dtCheckEventAndActivity.Rows[0]["EventType"]);
                                    fusebillCustomerDetail.Amount = Convert.ToDecimal(dtCheckEventAndActivity.Rows[0]["Amount"]);
                                    fusebillCustomerDetail.TransactionId = Convert.ToInt32(dtCheckEventAndActivity.Rows[0]["TransactionId"]);
                                    fusebillCustomerDetail.InvoiceAllocations = Convert.ToString(dtCheckEventAndActivity.Rows[0]["InvoiceAllocations"]);
                                }

                                DataTable dtCheckPaymentMethodExists = ds.Tables[2];
                                fusebillCustomerDetail.PaymentMethodExist = Convert.ToBoolean(dtCheckPaymentMethodExists.Rows[0]["PaymentMethodExist"]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "ValidateCustomer", ex.Message);
            }
            return fusebillCustomerDetail;
        }

        private void CustomerUpdate(int CompanyId, int FusebillId)
        {
            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.SL_RegisterCustomerUpdateProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = CompanyId;
                    cmd.Parameters.Add("@FusebillId", SqlDbType.Int).Value = FusebillId;
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "ActivateCustomer", ex.Message);
                throw ex;
            }
        }


        private void DeleteCustomer(FusebillCustomerDetail fusebillCustomerDetail)
        {
            try
            {
                if (fusebillCustomerDetail != null)
                {
                    string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                    //Sql Connection to pass request and fetch response  
                    using (SqlConnection connection = new SqlConnection(strSqlConnection))
                    {
                        //Calling the stored procedure with all the required paramaters 
                        SqlCommand cmd = new SqlCommand(Keys.SL_RegisterCustomerStatusCheckProcedure, connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 300;

                        cmd.Parameters.Add("@FusebillId", SqlDbType.Int).Value = fusebillCustomerDetail.CustomerId;
                        cmd.Parameters.Add("@PaymentActivityId", SqlDbType.Int).Value = fusebillCustomerDetail.PaymentActivityId;
                        //cmd.Parameters.Add("@JsonText", SqlDbType.NVarChar).Value = fusebillCustomerDetail.CustomerDetails;

                        connection.Open();
                        using (SqlDataReader dbReader = cmd.ExecuteReader())
                        {
                            while (dbReader.Read())
                            {
                                string strResult = Convert.ToString(dbReader["SPResult"]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "DeleteCustomer", ex.Message);
            }
        }

        private void AddSubscription(FusebillSubscriptionReq fusebillSubscriptionReq, FusebillSubscriptionResp fusebillSubscriptionResp)
        {
            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerGetFuseBillKeyValueProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@KeyName", SqlDbType.NVarChar).Value = "'APIKey', 'SubscriptionsCreateUrl', 'SubscriptionsActivationUrl'";
                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            var dataRowApiKey = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "APIKey").FirstOrDefault();
                            string apiKey = Convert.ToString(dataRowApiKey["Value"]);

                            var dataRowSubscriptionsCreateUrl = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "SubscriptionsCreateUrl").FirstOrDefault();
                            string subscriptionsCreateUrl = Convert.ToString(dataRowSubscriptionsCreateUrl["Value"]);

                            List<string> subscriptionIds = new List<string>();

                            foreach (var item in fusebillSubscriptionReq.fusebillSubscriptionRequest)
                            {
                                //  TO DO comment if not required to reduce calls
                                ValidateCouponSubscription(item);
                                SaveCouponDetails(item.customerId, 0, item.couponCodes);

                                string jsonData = JsonConvert.SerializeObject(item);
                                WebRequest request = WebRequest.Create(subscriptionsCreateUrl);
                                request.ContentType = "application/json";
                                request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                                request.Method = "POST";
                                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                                {
                                    streamWriter.Write(jsonData);
                                    streamWriter.Flush();
                                    streamWriter.Close();
                                }

                                var httpResponse = (HttpWebResponse)request.GetResponse();
                                //Record the response from our request
                                var result = "";
                                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    result = streamReader.ReadToEnd();
                                    JsonDocument doc = JsonDocument.Parse(result);
                                    JsonElement root = doc.RootElement;
                                    JsonElement jecustomerId = root.GetProperty("customerId");
                                    JsonElement jeplanRevisionId = root.GetProperty("planFrequency").GetProperty("planRevisionId");
                                    subscriptionIds.Add(jeplanRevisionId.ToString());
                                }
                            }

                            // TO DO - Activate Customer in Fusebill system - Modify key in database for Activation
                            //ActivateCustomerInFusebill(fusebillSubscriptionReq.customerId);

                            // TO DO - commenting this as it will activate subscription in fusebill unnecessary 
                            //ActivateSubscription(subscriptionIds);
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "CreateSubscription", ex.Message);
                throw;
            }
        }

        private List<Subscriptions> GetActiveSubscriptions()
        {
            var subscriptionList = new List<Subscriptions>();
            string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

            //Sql Connection to pass request and fetch response  
            using (SqlConnection connection = new SqlConnection(strSqlConnection))
            {
                //Calling the stored procedure with all the required paramaters 
                SqlCommand cmd = new SqlCommand(Keys.SL_GetSubscriptionsProcedure, connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;

                connection.Open();
                using (SqlDataReader dbReader = cmd.ExecuteReader())
                    while (dbReader.Read())
                    {
                        var data = new Subscriptions()
                        {
                            SubscriptionId = Convert.ToInt32(dbReader["SubscriptionId"]),
                            SubscriptionName = Convert.ToString(dbReader["SubscriptionName"])
                        };
                        subscriptionList.Add(data);
                    }
                return subscriptionList;
            }
        }

        private List<FusebillProductSubscriptions> GetFusebillProductSubscriptions(string FranchiseCode)
        {
            //string FranchiseCode, int FusebillProductId
            var fusebillSubscriptionList = new List<FusebillProductSubscriptions>();
            string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

            //Sql Connection to pass request and fetch response  
            using (SqlConnection connection = new SqlConnection(strSqlConnection))
            {
                //Calling the stored procedure with all the required paramaters 
                SqlCommand cmd = new SqlCommand(Keys.SL_GetFusebillProductSubscriptionsProcedure, connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;

                cmd.Parameters.Add("@FranchiseCode", SqlDbType.VarChar).Value = FranchiseCode;
                //cmd.Parameters.Add("@FusebillProductId", SqlDbType.Int).Value = FusebillProductId;

                connection.Open();
                using (SqlDataReader dbReader = cmd.ExecuteReader())
                    while (dbReader.Read())
                    {
                        var data = new FusebillProductSubscriptions()
                        {
                            FranchiseCode = Convert.ToString(dbReader["FranchiseCode"]),
                            FusebillProductId = Convert.ToInt32(dbReader["FusebillProductId"]),
                            FusebillProductName = Convert.ToString(dbReader["FusebillProductName"]),
                            SubscriptionId = Convert.ToInt32(dbReader["SubscriptionId"]),
                            SubscriptionType = Convert.ToString(dbReader["SubscriptionType"]),
                            EquipmentTypeId = Convert.ToInt32(dbReader["EquipmentTypeId"])
                        };
                        fusebillSubscriptionList.Add(data);
                    }
                return fusebillSubscriptionList;
            }
        }

        private void RefundAmount(FusebillCustomerDetail fusebillCustomerDetail)
        {
            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                int originalPaymentId = fusebillCustomerDetail.TransactionId;
                decimal Amount = fusebillCustomerDetail.Amount;
                string refundAllocations = string.Empty;
                if (fusebillCustomerDetail.InvoiceAllocations != null && fusebillCustomerDetail.InvoiceAllocations.Length > 0)
                {
                    JsonDocument doc = JsonDocument.Parse(fusebillCustomerDetail.InvoiceAllocations);
                    JsonElement root = doc.RootElement;
                    List<RefundAllocations> lstRefundAllocations = new List<RefundAllocations>();
                    for (int i = 0; i < root.GetArrayLength(); i++)
                    {
                        RefundAllocations objRefundAllocations = new RefundAllocations();
                        JsonElement jeStatus = root[i].GetProperty("invoiceId");
                        JsonElement jePlanId = root[i].GetProperty("amount");
                        objRefundAllocations.InvoiceId = root[i].GetProperty("invoiceId").GetInt32();
                        objRefundAllocations.Amount = root[i].GetProperty("amount").GetDecimal();
                        lstRefundAllocations.Add(objRefundAllocations);
                    }
                    refundAllocations = JsonConvert.SerializeObject(lstRefundAllocations);                  
                }

                // Refund allocations
                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerGetFuseBillKeyValueProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@KeyName", SqlDbType.NVarChar).Value = "'APIKey', 'RefundUrl'";
                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            var dataRowApiKey = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "APIKey").FirstOrDefault();
                            string apiKey = Convert.ToString(dataRowApiKey["Value"]);

                            var dataRowRefundUrl = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "RefundUrl").FirstOrDefault();
                            string refundUrl = Convert.ToString(dataRowRefundUrl["Value"]);

                            string jsonData = string.Empty;
                            if (!string.IsNullOrWhiteSpace(refundAllocations))
                                jsonData = "{originalPaymentId:" + originalPaymentId + ",reference:'this is a refund',amount:" + Amount + ",refundAllocations:" + refundAllocations + "}";
                            else
                                jsonData = "{originalPaymentId:" + originalPaymentId + ",reference:'this is a refund',amount:" + Amount + "}";        
                            WebRequest request = WebRequest.Create(refundUrl);
                            request.ContentType = "application/json";
                            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                            request.Method = "POST";
                            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                            {
                                streamWriter.Write(jsonData);
                                streamWriter.Flush();
                                streamWriter.Close();
                            }

                            var httpResponse = (HttpWebResponse)request.GetResponse();
                            var result = "";
                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                result = streamReader.ReadToEnd();
                            }
                        }
                    }
                }

            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "RefundAmount", ex.Message);
                throw;
            }
        }

        private int ValidatePaymentActivities(int CustomerId)
        {
            try
            {
                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerGetFuseBillKeyValueProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@KeyName", SqlDbType.NVarChar).Value = "'APIKey', 'PaymentActvitiesUrl'";
                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            var dataRowApiKey = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "APIKey").FirstOrDefault();
                            string apiKey = Convert.ToString(dataRowApiKey["Value"]);

                            var dataRowPaymentActvitiesUrl = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "PaymentActvitiesUrl").FirstOrDefault();
                            string paymentActvitiesUrl = Convert.ToString(dataRowPaymentActvitiesUrl["Value"]);
                            paymentActvitiesUrl = paymentActvitiesUrl.Replace("{customerId}", CustomerId.ToString());
                            int paymentActivityId = 0;
                            WebRequest request = WebRequest.Create(paymentActvitiesUrl);
                            request.ContentType = "application/json";
                            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                            request.Method = "GET";

                            var httpResponse = (HttpWebResponse)request.GetResponse();
                            var result = "";
                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                result = streamReader.ReadToEnd();
                                if (result != null && result.Length > 2)
                                {
                                    JsonDocument doc = JsonDocument.Parse(result);
                                    JsonElement root = doc.RootElement;
                                    var u1 = root[0];
                                    JsonElement jeId = u1.GetProperty("id");
                                    paymentActivityId = Convert.ToInt32(jeId.ToString());
                                    return paymentActivityId;
                                }                           
                            }
                            return paymentActivityId;
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "ValidatePaymentActivities", ex.Message);
                throw;
            }
        }


        #region Encrption

        byte[] KEY_64 = { 42, 16, 93, 156, 78, 4, 218, 32 };
        byte[] IV_64 = { 55, 103, 246, 79, 36, 99, 167, 3 };

        private string SFF_ENCRYPT(string value)
        {
            if ((value != ""))
            {
                DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, cryptoProvider.CreateEncryptor(KEY_64, IV_64), CryptoStreamMode.Write);
                StreamWriter sw = new StreamWriter(cs);
                string return_string;
                sw.Write(value);
                sw.Flush();
                cs.FlushFinalBlock();
                ms.Flush();
                return_string = Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
                return return_string;
            }
            else
            {
                return "";
            }
        }
        #endregion

    }
}
