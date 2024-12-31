using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ServiceFabricApp.API.Model;
using ServiceFabricApp.API.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace ServiceFabricApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FuseBillSubscriptionController : ControllerBase
    {

        /// <summary>
        /// Configuration
        /// </summary>
        private readonly IConfiguration Configuration;

        /// <summary>
        /// FuseBillPaymentController
        /// </summary>
        /// <param name="_configuration"></param>
        public FuseBillSubscriptionController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        /// <summary>
        /// ObjFuseBillSubscriptionRequest
        /// </summary>
        /// <param name="ObjFuseBillSubscriptionRequest"></param>
        /// <returns></returns>
        [EnableCors("customPolicy")]
        [Route("SaveUpdateSubscription")]
        [HttpPost]
        public FuseBillSubscriptionResponse SaveUpdateSubscription(FuseBillSubscriptionRequest ObjFuseBillSubscriptionRequest)
        {
            FuseBillSubscriptionResponse ObjFuseBillSubscriptionResponse = new FuseBillSubscriptionResponse();
            try
            {
                Common.WriteLog("FuseBillSubscriptionController", "SaveUpdateSubscription Success", "Subscription method called");
                Common.WriteLog("FuseBillSubscriptionController", "SaveUpdateSubscription Success", ObjFuseBillSubscriptionRequest.ToString());
                string headers = String.Empty;
                foreach (var key in Request.Headers.Keys)
                    headers += key + "=" + Request.Headers[key] + Environment.NewLine;

                Common.WriteLog("FuseBillSubscriptionController", "SaveUpdateFuseBillSubscription Header", headers);

                //Getting web hook request for logging
                string jsonDataSubscription = JsonConvert.SerializeObject(ObjFuseBillSubscriptionRequest);
                Common.WriteLog("FuseBillSubscriptionController", "SaveUpdateFuseBillSubscription", jsonDataSubscription);

                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                //Sql Connection to pass request and fetch response  
                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerSaveUpdateSubscriptionProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@EventType", SqlDbType.NVarChar).Value = ObjFuseBillSubscriptionRequest.EventType;
                    cmd.Parameters.Add("@EventSource", SqlDbType.NVarChar).Value = ObjFuseBillSubscriptionRequest.EventSource;

                    cmd.Parameters.Add("@FusebillId", SqlDbType.Int).Value = ObjFuseBillSubscriptionRequest.Subscription.customerId;
                    cmd.Parameters.Add("@SubscriptionId", SqlDbType.NVarChar).Value = ObjFuseBillSubscriptionRequest.Subscription.id;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar).Value = ObjFuseBillSubscriptionRequest.Subscription.status;

                    cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = ObjFuseBillSubscriptionRequest.Subscription.amount;
                    cmd.Parameters.Add("@NetMrr", SqlDbType.Float).Value = ObjFuseBillSubscriptionRequest.Subscription.netMrr;

                    connection.Open();

                    using (SqlDataReader dbReader = cmd.ExecuteReader())
                    {
                        while (dbReader.Read())
                        {
                            ObjFuseBillSubscriptionResponse.SPResult = Convert.ToString(dbReader["SPResult"]);
                            if (Convert.ToInt32(ObjFuseBillSubscriptionResponse.SPResult) == 0)
                                HttpContext.Response.StatusCode = 500;
                            else
                                HttpContext.Response.StatusCode = 200;
                        }
                    }
                }

                // Save Subscription details
                SaveSubscriptionDetails(ObjFuseBillSubscriptionRequest.Subscription.customerId);
                // Save and Update Customer subscriptions
                SaveUpdateCustomerSubscription(ObjFuseBillSubscriptionRequest.Subscription.customerId);
                // Delete fusebill coupon detail on Cancellation 
                DeleteFuseBillCouponDetail(ObjFuseBillSubscriptionRequest.Subscription.customerId);
                Common.WriteLog("FuseBillSubscriptionController", "SaveUpdateFuseBillSubscription SPResult", ObjFuseBillSubscriptionResponse.SPResult);
            }
            catch (Exception ex) //Logging the Error into file while exception
            {
                Common.WriteLog("FuseBillSubscriptionController", "SaveUpdateFuseBillSubscription SPResult", ObjFuseBillSubscriptionResponse.SPResult);
                Common.WriteLog("FuseBillSubscriptionController", "SaveUpdateFuseBillSubscription Failed", ex.Message);
            }
            return ObjFuseBillSubscriptionResponse;
        }

        private void SaveSubscriptionDetails(int CustomerId)
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

                    cmd.Parameters.Add("@KeyName", SqlDbType.NVarChar).Value = "'APIKey'";

                    connection.Open();
                    string apiKey = string.Empty;
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            var dataRowApiKey = dt.AsEnumerable().Where(x => x.Field<string>("KeyName") == "APIKey").FirstOrDefault();
                            apiKey = Convert.ToString(dataRowApiKey["Value"]);
                        }
                    }

                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd2 = new SqlCommand(Keys.RegisterCustomerSubscriptionsProcedure, connection);
                    cmd2.CommandType = CommandType.StoredProcedure;
                    cmd2.CommandTimeout = 300;

                    cmd2.Parameters.Add("@FusebillId", SqlDbType.Int).Value = CustomerId;
                    using (SqlDataAdapter sda2 = new SqlDataAdapter(cmd2))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda2.Fill(dt);

                            foreach (DataRow row in dt.Rows)
                            {
                                int SubscriptionId = Convert.ToInt32(row["SubscriptionId"]);
                                //Configure URI
                                WebRequest request = WebRequest.Create("HTTPS://secure.fusebill.com/v1/subscriptions/" + SubscriptionId);
                                //Add Content type
                                request.ContentType = "application/json";
                                //Add Api key authorization
                                request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + apiKey);
                                //Set request method
                                request.Method = "GET";
                                //Perform the request
                                HttpWebResponse httpResponse;
                                try
                                {
                                    //Perform the request
                                    httpResponse = (HttpWebResponse)request.GetResponse();
                                }
                                catch
                                {
                                    Common.WriteLog("FuseBillSubscriptionController", "Subscription delete scenario continue " + SubscriptionId, "");
                                    continue;
                                }

                                //Record the response from our request
                                var result = "";
                                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    result = streamReader.ReadToEnd();
                                    JsonDocument doc = JsonDocument.Parse(result);
                                    JsonElement root = doc.RootElement;

                                    JsonElement jecustomerId = root.GetProperty("customerId");
                                    JsonElement jeSubscriptionStatus = root.GetProperty("status");
                                    JsonElement jeplanRevisionId = root.GetProperty("planFrequency").GetProperty("planRevisionId");
                                    JsonElement jePlanName = root.GetProperty("planName");
                                    JsonElement jePlanFrequencyId = root.GetProperty("planFrequency").GetProperty("id");

                                    for (int i = 0; i < root.GetProperty("subscriptionProducts").GetArrayLength(); i++)
                                    {
                                        var u1 = root.GetProperty("subscriptionProducts")[i];
                                        JsonElement jeSubscriptionId = u1.GetProperty("subscriptionId");

                                        JsonElement jeStatus = u1.GetProperty("planProduct").GetProperty("status");

                                        JsonElement jePlanId = u1.GetProperty("planProduct").GetProperty("planId");
                                        JsonElement jeProductId = u1.GetProperty("planProduct").GetProperty("productId");
                                        JsonElement jeProductName = u1.GetProperty("planProduct").GetProperty("productName");

                                        JsonElement jeQuantity = u1.GetProperty("planProduct").GetProperty("quantity");
                                        JsonElement jeAmount = u1.GetProperty("planProduct").GetProperty("orderToCashCycles")[0].GetProperty("pricingModel").GetProperty("quantityRanges")[0].GetProperty("prices")[0].GetProperty("amount");
                                        JsonElement jeCurrency = u1.GetProperty("planProduct").GetProperty("orderToCashCycles")[0].GetProperty("pricingModel").GetProperty("quantityRanges")[0].GetProperty("prices")[0].GetProperty("currency");
                                        JsonElement jeSubscriptionProductId = u1.GetProperty("planProduct").GetProperty("orderToCashCycles")[0].GetProperty("pricingModel").GetProperty("quantityRanges")[0].GetProperty("prices")[0].GetProperty("id");

                                        JsonElement jePlanProductUniqueId = u1.GetProperty("planProduct").GetProperty("planProductUniqueId");
                                        JsonElement jeNextRechargeDate = root.GetProperty("nextRechargeDate");
                                        JsonElement jeLastPurchaseDate = u1.GetProperty("lastPurchaseDate");

                                        //Calling the stored procedure with all the required paramaters 
                                        SqlCommand cmd1 = new SqlCommand(Keys.FusebillSaveUpdateSubscriptionDetailProcedure, connection);
                                        cmd1.CommandType = CommandType.StoredProcedure;
                                        cmd1.CommandTimeout = 300;

                                        cmd1.Parameters.Add("@FusebillId", SqlDbType.Int).Value = jecustomerId.GetInt32();
                                        cmd1.Parameters.Add("@SubscriptionId", SqlDbType.Int).Value = jeSubscriptionId.GetInt32();
                                        cmd1.Parameters.Add("@SubscriptionProductId", SqlDbType.Int).Value = jeSubscriptionProductId.GetInt32();

                                        cmd1.Parameters.Add("@Quantity", SqlDbType.Float).Value = jeQuantity.GetDouble();
                                        cmd1.Parameters.Add("@Price", SqlDbType.Float).Value = jeAmount.GetDouble();
                                        cmd1.Parameters.Add("@Currency", SqlDbType.NVarChar).Value = jeCurrency.GetString();

                                        if (Convert.ToString(jeSubscriptionStatus) == "Cancelled")
                                            cmd1.Parameters.Add("@Status", SqlDbType.NVarChar).Value = "Cancelled";
                                        else
                                        {
                                            if (jeQuantity.GetDouble() != 0.0)
                                                cmd1.Parameters.Add("@Status", SqlDbType.NVarChar).Value = "Active";
                                            else
                                                cmd1.Parameters.Add("@Status", SqlDbType.NVarChar).Value = "InActive";
                                        }

                                        cmd1.Parameters.Add("@PlanId", SqlDbType.Int).Value = jePlanId.GetInt32();
                                        cmd1.Parameters.Add("@PlanName", SqlDbType.NVarChar).Value = jePlanName.GetString();
                                        cmd1.Parameters.Add("@PlanFrequencyId", SqlDbType.Int).Value = jePlanFrequencyId.GetInt32();
                                        cmd1.Parameters.Add("@ProductId", SqlDbType.Int).Value = jeProductId.GetInt32();
                                        cmd1.Parameters.Add("@ProductName", SqlDbType.NVarChar).Value = jeProductName.GetString();
                                        cmd1.Parameters.Add("@PlanProductUniqueId", SqlDbType.Int).Value = jePlanProductUniqueId.GetInt32();
                                        cmd1.Parameters.Add("@NextRechargeDate", SqlDbType.DateTime).Value = jeNextRechargeDate.GetDateTime();
                                        if(jeLastPurchaseDate.ValueKind != JsonValueKind.Null)
                                            cmd1.Parameters.Add("@LastPurchaseDate", SqlDbType.DateTime).Value = jeLastPurchaseDate.GetDateTime();


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
                                // End parsing logic
                            }
                        }
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("FuseBillSubscriptionController", "SaveSubscriptionDetails", ex.Message);
            }
        }
        private void SaveUpdateCustomerSubscription(int customerId)
        {
            DataSet dsSubscriptions = new DataSet();
            try
            {
                CompanyDetails companyDetails = GetCompanyCompanyId(customerId);
                if (companyDetails.CompanyId > 0)
                {
                    SubscriptionModel subscriptionModel = new SubscriptionModel();
                    List<SubscriptionKeyValue> subscriptionKeyValueList = new List<SubscriptionKeyValue>();
                    List<SubscriptionType> subscriptionTypeList = GetSubscriptions(companyDetails.CompanyId);
                    List<FusebillProductSubscriptions> fusebillSubscriptions = GetFusebillProductSubscriptions(companyDetails.FranchaiseCode);
                    DataSet ds = GetSubscriptionsDataSet(customerId);
                    if (ds.Tables.Count > 0)
                    {
                        foreach (var item in subscriptionTypeList)
                        {
                            SubscriptionKeyValue subscriptionKeyValue = new SubscriptionKeyValue();
                            subscriptionKeyValue.Key = Convert.ToString(item.Id);
                            var drSubscriptions = ds.Tables[1].Select("Status='Active' AND ProductName='" + item.Subscription_Type + "'").Distinct().ToList();
                            if (drSubscriptions.Count == 0)
                            {
                                foreach (var item1 in fusebillSubscriptions)
                                {
                                    if (item1.SubscriptionType == item.Subscription_Type)
                                    {
                                        drSubscriptions = ds.Tables[1].Select("Status='Active' AND ProductId='" + item1.FusebillProductId + "'").Distinct().ToList();
                                        if(drSubscriptions.Count > 0)
                                            break;
                                    }
                                }
                            }
                            if (drSubscriptions.Count > 0)
                                subscriptionKeyValue.Value = true;
                            else
                                subscriptionKeyValue.Value = false;
                            subscriptionKeyValueList.Add(subscriptionKeyValue);
                        }
                        subscriptionModel.companyId = companyDetails.CompanyId;
                        subscriptionModel.subscriptionList = subscriptionKeyValueList;

                        UpdateSubscription(subscriptionModel);
                    }
                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "GetSubscriptionsDataSet", ex.Message);
            }
        }
        private CompanyDetails GetCompanyCompanyId(int fusebillId)
        {
            //int companyId = 0;
            CompanyDetails companyDetails = new CompanyDetails();
            try
            {
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                //Sql Connection to pass request and fetch response  
                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerCheckCompanyIdProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@FusebillId", SqlDbType.Int).Value = fusebillId;

                    connection.Open();
                    using (SqlDataReader dbReader = cmd.ExecuteReader())
                        while (dbReader.Read())
                        {
                            companyDetails.CompanyId = Convert.ToInt32(dbReader["CompanyId"]);
                            companyDetails.FranchaiseCode = Convert.ToString(dbReader["FranchiseCode"]);
                            companyDetails.IsFusebillEnable = Convert.ToBoolean(dbReader["IsFusebillEnable"]);
                        }

                }
            }
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("CustomerController", "GetCompanyCompanyId", ex.Message);
                throw ex;
            }
            return companyDetails;
        }
        private List<SubscriptionType> GetSubscriptions(int CompanyId)
        {
            var subscriptionList = new List<SubscriptionType>();
            string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

            //Sql Connection to pass request and fetch response  
            using (SqlConnection connection = new SqlConnection(strSqlConnection))
            {
                //Calling the stored procedure with all the required paramaters 
                SqlCommand cmd = new SqlCommand("GetSubscription", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;

                cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = CompanyId;

                connection.Open();
                using (SqlDataReader dbReader = cmd.ExecuteReader())
                    while (dbReader.Read())
                    {
                        var data = new SubscriptionType()
                        {
                            Id = Convert.ToInt32(dbReader["id"]),
                            Subscription_Type = Convert.ToString(dbReader["Subscription_type"]),
                            SubscriptionId = (dbReader["SubscriptionID"] != DBNull.Value) ? Convert.ToInt32(dbReader["SubscriptionID"]) : 0                           
                            //SubscriptionId = Convert.ToInt32(dbReader["SubscriptionID"])
                            //CompanyId = Convert.ToInt32(dbReader["CompanyID"]),
                            //IsChecked = Convert.ToBoolean(dbReader["IsChecked"])
                    };
                    
                        subscriptionList.Add(data);
                    }
                return subscriptionList;
            }
        }
        private void UpdateSubscription(SubscriptionModel model)
        {
            SqlConnection dbConnection = new SqlConnection(Configuration.GetConnectionString("DefaultConnection"));
            SqlCommand dbCommand = new SqlCommand("UpdateSubscription", dbConnection);

            dbCommand.CommandTimeout = 300;
            dbConnection.Open();
            SqlTransaction transaction;
            transaction = dbConnection.BeginTransaction();
            for (int value = 0; value <= model.subscriptionList.Count - 1; value++)
            {
                dbCommand.Transaction = transaction;
                dbCommand.Parameters.Clear();
                dbCommand.Parameters.AddWithValue("@companyId", model.companyId);
                dbCommand.Parameters.AddWithValue("@key", model.subscriptionList.ElementAt(value).Key);
                dbCommand.Parameters.AddWithValue("@value", model.subscriptionList.ElementAt(value).Value);
                dbCommand.CommandType = CommandType.StoredProcedure;
                dbCommand.ExecuteNonQuery();
            }
            transaction.Commit();
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
        private void DeleteFuseBillCouponDetail(int fusebillId)
        {
            try
            {
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                //Sql Connection to pass request and fetch response  
                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.FusebillDeleteFuseBillCouponDetailProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@FusebillId", SqlDbType.Int).Value = fusebillId;
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
            catch (Exception ex) //Logging the error into file while exception
            {
                Common.WriteLog("FuseBillSubscriptionController", "DeleteFuseBillCouponDetail", ex.Message);
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


    }
}
