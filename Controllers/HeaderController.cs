using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ServiceFabricApp.API.Model;
using ServiceFabricApp.API.Repositories;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace ServiceFabricApp.API.Controllers
{
    /// <summary>
    /// HeaderControllers
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HeaderController : Controller
    {
        /// <summary>
        /// Configuration
        /// </summary>
        private readonly IConfiguration Configuration;

        /// <summary>
        /// HeaderController
        /// </summary>
        /// <param name="_configuration"></param>
        public HeaderController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }


        /// <summary>
        /// GetHeaderDetailsResponse
        /// </summary>
        /// <returns></returns>
        [EnableCors("customPolicy")]
        [HttpPost]
        public HeaderResponse GetHeaderDetailsResponse()
        {
            HeaderResponse objHeaderDetailsResponse = new HeaderResponse();

            try
            {
                //Get CompanyId, UserId from header token
                int CompanyId = 0;
                int UserId = 0;
                if (!String.IsNullOrEmpty(Request.Headers["CompanyId"]))
                    CompanyId = Convert.ToInt32(Request.Headers["CompanyId"]);

                if (!String.IsNullOrEmpty(Request.Headers["UserId"]))
                    UserId = Convert.ToInt32(Request.Headers["UserId"]);


                //Getting the connection string from appsettings.json
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                //Get absolute path URL for Storeflix Homepage link from appsettings.json
                string baseURL = Configuration.GetSection("StoreflixServerURL")["DefaultURL"];

                //Sql Connection to pass request and fetch response  
                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.HeaderDetailsProcedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = CompanyId;
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;

                    connection.Open();

                    DataTable dtHeaderMenu = new DataTable();
                    List<HeaderMenuModels> menuList = new List<HeaderMenuModels>();

                    using (SqlDataReader dbReader = cmd.ExecuteReader())
                    {
                        while (dbReader.Read())
                        {
                            {
                                objHeaderDetailsResponse.UserLoginName = Convert.ToString(dbReader["UserLoginName"]);
                                objHeaderDetailsResponse.UserFirstName = Convert.ToString(dbReader["UserFirstName"]);
                                objHeaderDetailsResponse.UserLastName = Convert.ToString(dbReader["UserLastName"]);
                                //objHeaderDetailsResponse.CompanyLogo = baseURL + "/images/companylogos/" + Convert.ToString(dbReader["CompanyLogo"]);
                                objHeaderDetailsResponse.CompanyLogo =  Convert.ToString(dbReader["CompanyLogo"]);
                                objHeaderDetailsResponse.IsSuperAdmin = Convert.ToBoolean(dbReader["IsSuperAdmin"]);
                                objHeaderDetailsResponse.ThemeName = Convert.ToString(dbReader["ThemeName"]);
                            };
                        }

                        if(!string.IsNullOrEmpty(baseURL))
                        {
                            objHeaderDetailsResponse.Home = baseURL.ToString();
                        }
                        else
                        {
                            objHeaderDetailsResponse.Home = null;
                        }

                        dbReader.NextResult();

                        if (dbReader.HasRows == true)
                        {
                            dtHeaderMenu.Load(dbReader);
                        }
                        else
                        {
                            dbReader.NextResult();
                        }

                        if (dtHeaderMenu.Rows.Count > 0)
                        {
                            foreach (DataRow row in dtHeaderMenu.Rows)
                            {
                                var Menu = new HeaderMenuModels();

                                Menu.id = Convert.ToInt32(row["id"]);
                                Menu.MenuName = Convert.ToString(row["MenuName"]);
                                Menu.IsMainMenu = Convert.ToBoolean(row["IsMainMenu"]);
                                Menu.ParentId = row["ParentId"] == DBNull.Value ? 0 : Convert.ToInt32(row["ParentId"]);
                                if (!string.IsNullOrEmpty(row["MenuURL"].ToString()))
                                {
                                    if (Common.IsAbsolutePath(row["MenuURL"].ToString()))
                                        Menu.MenuURL = Convert.ToString(row["MenuURL"]);
                                    else
                                        Menu.MenuURL = baseURL + Convert.ToString(row["MenuURL"]);
                                }
                                else
                                {
                                    Menu.MenuURL = string.Empty;
                                }
                                Menu.DisplayName = Convert.ToString(row["DisplayName"]);
                                Menu.MenuOrder = row["MenuOrder"] == DBNull.Value ? 0 : Convert.ToInt32(row["MenuOrder"]);
                                menuList.Add(Menu);
                            }
                            objHeaderDetailsResponse.HeaderMenuList = menuList;
                        }
                        else
                        {
                            objHeaderDetailsResponse.HeaderMenuList = null;
                        }
                    }
                    connection.Close();
                    connection.Dispose();
                }
            }
            catch (Exception ex) //Logging the error into file while any exception occures
            {
                Common.WriteLog("HeaderController", "GetHeaderDetailsResponse", ex.Message);
            }

            return objHeaderDetailsResponse;
        }
    }
}
