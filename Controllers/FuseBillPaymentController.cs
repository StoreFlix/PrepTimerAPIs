using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ServiceFabricApp.API.Model;
using ServiceFabricApp.API.Repositories;
using System;
using System.Data;
using System.Data.SqlClient;

namespace ServiceFabricApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FuseBillPaymentController : ControllerBase
    {
        /// <summary>
        /// Configuration
        /// </summary>
        private readonly IConfiguration Configuration;

        /// <summary>
        /// FuseBillPaymentController
        /// </summary>
        /// <param name="_configuration"></param>
        public FuseBillPaymentController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        /// <summary>
        /// SaveUpdateFuseBillPayment
        /// </summary>
        /// <param name="ObjFuseBillPaymentRequest"></param>
        [EnableCors("customPolicy")]
        [Route("SaveUpdatePayment")]
        [HttpPost]
        public FuseBillPaymentResponse SaveUpdatePayment(FuseBillPaymentRequest ObjFuseBillPaymentRequest)
        {
            FuseBillPaymentResponse ObjFuseBillPaymentResponse = new FuseBillPaymentResponse();
            try
            {
                Common.WriteLog("FuseBillPaymentController", "SaveUpdatePayment Success", "Payment method called");
                //string headers = String.Empty;
                //foreach (var key in Request.Headers.Keys)
                //    headers += key + "=" + Request.Headers[key] + Environment.NewLine;

                //Common.WriteLog("FuseBillPaymentController", "SaveUpdatePayment Header", headers);

                //Getting web hook request for logging
                string jsonData = JsonConvert.SerializeObject(ObjFuseBillPaymentRequest);
                Common.WriteLog("FuseBillPaymentController", "SaveUpdateFuseBillPayment", jsonData);

                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                //Sql Connection to pass request and fetch response  
                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.RegisterCustomerSaveUpdatePaymentProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@EventType", SqlDbType.NVarChar).Value = ObjFuseBillPaymentRequest.EventType;
                    cmd.Parameters.Add("@EventSource", SqlDbType.NVarChar).Value = ObjFuseBillPaymentRequest.EventSource;

                    if (ObjFuseBillPaymentRequest.Payment != null)
                    {
                        cmd.Parameters.Add("@FusebillId", SqlDbType.Int).Value = ObjFuseBillPaymentRequest.Payment.customerId;
                        cmd.Parameters.Add("@PaymentActivityId", SqlDbType.Int).Value = ObjFuseBillPaymentRequest.Payment.paymentActivityId;
                        cmd.Parameters.Add("@Result", SqlDbType.NVarChar).Value = ObjFuseBillPaymentRequest.Payment.result;

                        cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = ObjFuseBillPaymentRequest.Payment.amount;
                        cmd.Parameters.Add("@UnAllocatedAmount", SqlDbType.Float).Value = ObjFuseBillPaymentRequest.Payment.unallocatedAmount;
                        cmd.Parameters.Add("@RefundableAmount", SqlDbType.Float).Value = ObjFuseBillPaymentRequest.Payment.refundableAmount;
                        cmd.Parameters.Add("@TransactionId", SqlDbType.Int).Value = ObjFuseBillPaymentRequest.Payment.id;
                        string jsonDataInvoiceAllocations = JsonConvert.SerializeObject(ObjFuseBillPaymentRequest.Payment.invoiceAllocations);
                        cmd.Parameters.Add("@InvoiceAllocations", SqlDbType.NVarChar).Value = jsonDataInvoiceAllocations;
                    }
                    else if (ObjFuseBillPaymentRequest.PaymentMethod != null)
                    {
                        cmd.Parameters.Add("@FusebillId", SqlDbType.Int).Value = ObjFuseBillPaymentRequest.PaymentMethod.customerId;
                    }

                    connection.Open();
                    using (SqlDataReader dbReader = cmd.ExecuteReader())
                    {
                        while (dbReader.Read())
                        {
                            ObjFuseBillPaymentResponse.SPResult = Convert.ToString(dbReader["SPResult"]);
                            if (Convert.ToInt32(ObjFuseBillPaymentResponse.SPResult) == 0)
                                HttpContext.Response.StatusCode = 500;
                            else
                                HttpContext.Response.StatusCode = 200;
                        }
                    }
                }
                Common.WriteLog("FuseBillPaymentController", "SaveUpdatePayment SPResult", ObjFuseBillPaymentResponse.SPResult);
            }
            catch (Exception ex) //Logging the Error into file while exception
            {
                Common.WriteLog("FuseBillPaymentController", "SaveUpdatePayment SPResult", ObjFuseBillPaymentResponse.SPResult);
                Common.WriteLog("FuseBillPaymentController", "SaveUpdatePayment Failed", ex.Message);
            }
            return ObjFuseBillPaymentResponse;
        }




    }
}
