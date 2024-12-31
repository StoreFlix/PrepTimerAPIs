using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceFabricAPIsOld.Model;
using System.Data.SqlClient;
using System.Data;
using ServiceFabricApp.API.Repositories;
using ServiceFabricAPIsOld.Repositories;
using Azure.Core;
namespace ServiceFabricAPIsOld.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        /// <summary>
        /// Configuration
        /// </summary>
        private readonly IConfiguration Configuration;


        /// <summary>
        /// UserController
        /// </summary>
        /// <param name="_configuration"></param>
        public UserController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        [EnableCors("customPolicy")]
        [Route("Login")]
        [HttpPost]
        public async Task<ActionResult> Login([FromBody] LoginModel loginModel)
        {
            string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

            int statusCode = 0;
            string responseMessage = string.Empty;
            DateTime? subscriptionEndDate= null;
            //Sql Connection to pass request and fetch response  
            using (SqlConnection connection = new SqlConnection(strSqlConnection))
            {
                //Calling the stored procedure with all the required paramaters 
                SqlCommand cmd = new SqlCommand("PT_ValidateUser", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;

                cmd.Parameters.Add("@ClientName", SqlDbType.VarChar).Value = loginModel.UserDetails.Email;
                cmd.Parameters.Add("@ClientSecret", SqlDbType.VarChar).Value = Common.SFF_ENCRYPT(loginModel.UserDetails.Password);
                connection.Open();

                using (SqlDataReader dbReader = await cmd.ExecuteReaderAsync())
                while (dbReader.Read())
                {

                        statusCode = Convert.ToInt32(dbReader["Status"]);
                        responseMessage = Convert.ToString(dbReader["Message"]);
                       
                 }

                if (statusCode == 0)
                {
                    return Unauthorized(new { message = responseMessage });
                }

                if (!loginModel.IsWebLogin.HasValue || loginModel.IsWebLogin.Value == false)
                {
                    cmd = new SqlCommand("PT_ValidateSubscriptions", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@ClientName", SqlDbType.VarChar).Value = loginModel.UserDetails.Email;
                    cmd.Parameters.Add("@DeviceId", SqlDbType.VarChar).Value = loginModel.DeviceDetails.DeviceUniqueId;
                    cmd.Parameters.Add("@DeviceType", SqlDbType.VarChar).Value = loginModel.DeviceDetails.DeviceType;
                    cmd.Parameters.Add("@DeviceOS", SqlDbType.VarChar).Value = loginModel.DeviceDetails.DeviceOS;
                    cmd.Parameters.Add("@DeviceIMEI", SqlDbType.VarChar).Value = loginModel.DeviceDetails.DeviceIMEI;


                    using (SqlDataReader dbReader = await cmd.ExecuteReaderAsync())
                        while (dbReader.Read())
                        {
                            statusCode = Convert.ToInt32(dbReader["Status"]);
                            responseMessage = Convert.ToString(dbReader["Message"]);
                            if (dbReader["SubscriptionEndDate"] != DBNull.Value)
                                subscriptionEndDate = Convert.ToDateTime(dbReader["SubscriptionEndDate"]);
                        }

                    if (statusCode == 0)
                    {
                        return Unauthorized(new { message = responseMessage });
                    }
                }

                TokenService tokenSVC = new TokenService(Configuration);
                var accessToken = tokenSVC.GenerateAccessToken(loginModel.UserDetails.Email);
                var refreshToken = tokenSVC.GenerateRefreshToken();


                return Ok(new { accessToken = accessToken, refreshToken = refreshToken, subscriptionEndDate= subscriptionEndDate });
            }

            
        }
    }
}
