using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceFabricAPIsOld.Model;
using System.Data.SqlClient;
using System.Data;
using ServiceFabricApp.API.Repositories;
using ServiceFabricAPIsOld.Repositories;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using PrepTimerAPIs.Models;
using PrepTimerAPIs.Services;
using PrepTimerAPIs.Dtos;
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

        private readonly IUserService _service;

        private readonly ICategoryService _ctservice;

        private readonly IItemService _itemservice;
        /// <summary>
        /// UserController
        /// </summary>
        /// <param name="_configuration"></param>
        public UserController(IConfiguration _configuration,IUserService service, ICategoryService ctservice, IItemService itemservice)
        {
            Configuration = _configuration;
            _service = service;
            _ctservice = ctservice;
            _itemservice = itemservice;
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
                    cmd.Parameters.Add("@DeviceModel", SqlDbType.VarChar).Value = loginModel.DeviceDetails.DeviceModel;
                    cmd.Parameters.Add("@DeviceManufacturer", SqlDbType.VarChar).Value = loginModel.DeviceDetails.DeviceManufacturer;



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

                if (loginModel.UserDetails.Email.Contains("antunes"))
                {
                    var _translations = await _ctservice.GetTranslations();
                    var _categories = await _ctservice.GetCategoriesAsync(1);
                    var _items = await _itemservice.GetItemsAsync(1);
                    return Ok(new { accessToken = accessToken, refreshToken = refreshToken, subscriptionEndDate = subscriptionEndDate, translations = _translations, categories = _categories, items = _items });
                }

                //var _translations = await _ctservice.GetTranslations();
                //var _categories = await _ctservice.GetCategoriesAsync(1);
                //var _items = await _itemservice.GetItemsAsync(1);

                //return Ok(new { accessToken = accessToken, refreshToken = refreshToken, subscriptionEndDate= subscriptionEndDate, translations = _translations, categories = _categories, items = _items });
                return Ok(new { accessToken = accessToken, refreshToken = refreshToken, subscriptionEndDate = subscriptionEndDate });

            }


        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<PTUser>>> GetUsersByCompany()
        {
            var companyId = 1;
            var users = await _service.GetUsersAsync(companyId);
            return Ok(users);
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<PTUser>>> AddUser([FromBody] UserDto dto)
        {
           await _service.AddUserAsync(dto);
            return Ok(new { message = "User added successfully." });
        }

        // PUT: api/Category/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] PTUser user)
        {
            if (id != user.UserId)
                return BadRequest("User ID mismatch");

            var result = await _service.UpdateUserAsync(user);

            if (!result)
                return NotFound();

            return NoContent();
        }

        // DELETE: api/Category/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _service.DeleteUserAsync(id);

            if (!result)
                return NotFound();

            return NoContent();
        }


        [EnableCors("customPolicy")]
        [Route("RefreshPrepTimerData")]
        [HttpGet]
        public async Task<ActionResult> RefreshPrepTimerData()
        {
            string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

            int statusCode = 0;
            string responseMessage = string.Empty;
            DateTime? subscriptionEndDate = null;
            //Sql Connection to pass request and fetch response  
            using (SqlConnection connection = new SqlConnection(strSqlConnection))
            {
                var _translations = await _ctservice.GetTranslations();
                var _categories = await _ctservice.GetCategoriesAsync(1);
                var _items = await _itemservice.GetItemsAsync(1);

                return Ok(new { translations = _translations, categories = _categories, items = _items });
            }


        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var result = await _service.ResetPasswordAsync(model.Token, model.NewPassword);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpPost("request-reset-password")]
        public async Task<IActionResult> RequestResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            var result = await _service.RequestPasswordResetAsync(request.Email);

            if (!result.IsSuccess)
                return NotFound(result.Message);

            return Ok(result.Message);
        }

        [Route("GetPassword")]
        [HttpGet]
        public async Task<IActionResult> GetPassword(string password)
        {
            var result = await _service.HashPassword(password);

            if (String.IsNullOrWhiteSpace(result))
                return NotFound();

            return Ok(result);
        }
    }
}
