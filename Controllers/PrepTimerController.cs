using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Azure.Storage;
using Azure.Storage.Blobs;

using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

using ServiceFabricApp.API.Model;
using ServiceFabricApp.API.Repositories;

namespace ServiceFabricApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrepTimerController : ControllerBase
    {
        /// <summary>
        /// Configuration
        /// </summary>
        private readonly IConfiguration Configuration;

        /// <summary>
        /// PrepTimerController
        /// </summary>
        /// <param name="_configuration"></param>
        public PrepTimerController(IConfiguration _configuration)
        {
            Configuration = _configuration;


        }

        [EnableCors("customPolicy")]
        [Route("AddUpdateCondimentCategory")]
        [HttpPost]
        public bool AddUpdateCondimentCategory(AddUpdateCondimentCategoryRequest addUpdateCondimentCategoryRequest)
        {
            bool isAddedSuccessfully = false;
            try
            {
                //CompanyId will get it from header token
                int CompanyId = 0;
                if (!String.IsNullOrEmpty(Request.Headers["CompanyId"]))
                    CompanyId = Convert.ToInt32(Request.Headers["CompanyId"]);

                //UserId will get it from header token
                int UserId = 0;
                if (!String.IsNullOrEmpty(Request.Headers["Userid"]))
                    UserId = Convert.ToInt32(Request.Headers["Userid"]);

                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.SL_PT_AddCondimentCategory, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = CompanyId;
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
                    cmd.Parameters.Add("@CategoryId", SqlDbType.Int).Value = addUpdateCondimentCategoryRequest.CategoryId;
                    cmd.Parameters.Add("@StartTime", SqlDbType.DateTime).Value = addUpdateCondimentCategoryRequest.StartTime;
                    cmd.Parameters.Add("@EndTime", SqlDbType.DateTime).Value = addUpdateCondimentCategoryRequest.EndTime;
                    cmd.Parameters.Add("@Status", SqlDbType.Bit).Value = addUpdateCondimentCategoryRequest.Status;

                    DataTable dtCategoryNames = Common.ToDataTable(addUpdateCondimentCategoryRequest.CategoryNames);

                    var categoryNames = new SqlParameter("@CategoryNames", SqlDbType.Structured);
                    categoryNames.TypeName = "dbo.[CategoryNames]";
                    categoryNames.Value = dtCategoryNames;
                    cmd.Parameters.Add(categoryNames);

                    connection.Open();
                    using (SqlDataReader dbReader = cmd.ExecuteReader())
                    {
                        while (dbReader.Read())
                        {
                            isAddedSuccessfully = Convert.ToString(dbReader["SPResult"]) == "1" ? true : false;
                        }
                    }
                }
            }
            catch (Exception ex)  //Logging the Error into file while exception
            {
                Common.WriteLog("PrepTimerController", "AddUpdateCondimentCategory", ex.Message);
            }
            return isAddedSuccessfully;
        }


        [EnableCors("customPolicy")]
        [Route("GetCondimentCategories")]
        [HttpPost]
        public List<CondimentCategoriesResponse> GetCondimentCategories()
        {
            List<CondimentCategoriesResponse> lstCondimentCategoriesResponse = new List<CondimentCategoriesResponse>();
            try
            {
                //CompanyId will get it from header token
                int CompanyId = 0;
                if (!String.IsNullOrEmpty(Request.Headers["CompanyId"]))
                    CompanyId = Convert.ToInt32(Request.Headers["CompanyId"]);

                //UserId will get it from header token
                int UserId = 0;
                if (!String.IsNullOrEmpty(Request.Headers["Userid"]))
                    UserId = Convert.ToInt32(Request.Headers["Userid"]);

                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.SL_PT_GetCondimentCategories, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = CompanyId;
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
                    cmd.Parameters.Add("@LangName", SqlDbType.NVarChar).Value = "English";


                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            lstCondimentCategoriesResponse = Common.ConvertDataTable<CondimentCategoriesResponse>(dt);
                        }
                    }
                }
            }
            catch (Exception ex)  //Logging the Error into file while exception
            {
                Common.WriteLog("PrepTimerController", "GetCondimentCategories", ex.Message);
            }
            return lstCondimentCategoriesResponse;
        }


        [EnableCors("customPolicy")]
        [Route("AddUpdateCondimentLanguage")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public IActionResult AddUpdateCondimentLanguage([FromForm]AddUpdateCondimentLanguagesRequest addUpdateCondimentLanguagesRequest)
        {
            bool isAddedSuccessfully = false;
            try
            {
                //CompanyId will get it from header token
                int CompanyId = 0;
                if (!String.IsNullOrEmpty(Request.Headers["CompanyId"]))
                    CompanyId = Convert.ToInt32(Request.Headers["CompanyId"]);

                //UserId will get it from header token
                int UserId = 0;
                if (!String.IsNullOrEmpty(Request.Headers["Userid"]))
                    UserId = Convert.ToInt32(Request.Headers["Userid"]);

                if (addUpdateCondimentLanguagesRequest.LangFile == null || addUpdateCondimentLanguagesRequest.LangFile.Length == 0)
                {
                    return BadRequest("File is required.");
                }

                String FileNameToUpload = String.Empty; FileNameToUpload = addUpdateCondimentLanguagesRequest.LangFile.Name;

                if (HttpContext.Request.Form.Files.Count() > 0)
                {
                    string GUID = System.Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(addUpdateCondimentLanguagesRequest.LangFile.FileName);
                    FileNameToUpload = GUID + "_" + Path.GetFileNameWithoutExtension(addUpdateCondimentLanguagesRequest.LangFile.FileName) + extension;

                    FileNameToUpload = "PrepTimer/Translations/" + FileNameToUpload;
                }
                string filepath = addUpdateCondimentLanguagesRequest.FilePath;
                if (HttpContext.Request.Form.Files.Count() > 0)
                    filepath = UpLoadFile(FileNameToUpload).Result;

                addUpdateCondimentLanguagesRequest.FilePath = filepath;

                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.PT_AddCondimentLanguages, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;
                   
                    cmd.Parameters.Add("@LangId", SqlDbType.Int).Value = addUpdateCondimentLanguagesRequest.Id;
                    cmd.Parameters.Add("@LangName", SqlDbType.NVarChar).Value = addUpdateCondimentLanguagesRequest.Name;
                    cmd.Parameters.Add("@Locale", SqlDbType.NVarChar).Value = addUpdateCondimentLanguagesRequest.Locale;
                    cmd.Parameters.Add("@FilePath", SqlDbType.NVarChar).Value = addUpdateCondimentLanguagesRequest.FilePath;

                    connection.Open();
                    using (SqlDataReader dbReader = cmd.ExecuteReader())
                    {
                        while (dbReader.Read())
                        {
                            isAddedSuccessfully = Convert.ToString(dbReader["SPResult"]) == "1" ? true : false;
                        }
                    }
                }
            }
            catch (Exception ex)  //Logging the Error into file while exception
            {
                Common.WriteLog("PrepTimerController", "AddUpdateCondimentLanguage", ex.Message);
            }
            return Ok(isAddedSuccessfully);
        }


        [EnableCors("customPolicy")]
        [Route("GetLanguages")]
        [HttpPost]
        public List<LanguagesResponse> GetLanguages()
        {
            List<LanguagesResponse> lstLanguagesResponse = new List<LanguagesResponse>();
            try
            {
                //CompanyId will get it from header token
                int CompanyId = 0;
                if (!String.IsNullOrEmpty(Request.Headers["CompanyId"]))
                    CompanyId = Convert.ToInt32(Request.Headers["CompanyId"]);

                //UserId will get it from header token
                int UserId = 0;
                if (!String.IsNullOrEmpty(Request.Headers["Userid"]))
                    UserId = Convert.ToInt32(Request.Headers["Userid"]);

                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.PT_GetLanguages, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    //cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = CompanyId;
                    //cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;

                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            lstLanguagesResponse = Common.ConvertDataTable<LanguagesResponse>(dt);
                        }
                    }
                }
            }
            catch (Exception ex)  //Logging the Error into file while exception
            {
                Common.WriteLog("PrepTimerController", "GetLanguages", ex.Message);
            }
            return lstLanguagesResponse;
        }


        [EnableCors("customPolicy")]
        [Route("AddUpdateCondimentCategoryMapping")]
        [HttpPost]
        public bool AddUpdateCondimentCategoryMapping()
        {
            bool isAddedSuccessfully = false;
            try
            {
                //CompanyId will get it from header token
                int CompanyId = 0;
                if (!String.IsNullOrEmpty(Request.Headers["CompanyId"]))
                    CompanyId = Convert.ToInt32(Request.Headers["CompanyId"]);

                //UserId will get it from header token
                int UserId = 0;
                if (!String.IsNullOrEmpty(Request.Headers["Userid"]))
                    UserId = Convert.ToInt32(Request.Headers["Userid"]);

                AddUpdateCondimentCategoryMappingRequest addUpdateCondimentCategoryMappingRequest = new AddUpdateCondimentCategoryMappingRequest();
                if (!String.IsNullOrEmpty(HttpContext.Request.Form["CondimentCategoryMappingRequest"]))
                {
                    var jsonObjectCondimentCategoryMappingRequest = HttpContext.Request.Form["CondimentCategoryMappingRequest"].ToString();
                    addUpdateCondimentCategoryMappingRequest = JsonConvert.DeserializeObject<AddUpdateCondimentCategoryMappingRequest>(jsonObjectCondimentCategoryMappingRequest);
                }

                string ImagePath = HttpContext.Request.Form["CondimentLogo"];
                string FileNameToUpload = string.Empty;
                if (HttpContext.Request.Form.Files.Count() > 0)
                {
                    var CondimentFile = HttpContext.Request.Form.Files[0];
                    string GUID = System.Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(CondimentFile.FileName);
                    FileNameToUpload = GUID + "_" + Path.GetFileNameWithoutExtension(CondimentFile.FileName) + extension;
                }
                string imageUrl = string.Empty;
                if (HttpContext.Request.Form.Files.Count() > 0)
                    imageUrl = UpLoadCondimentLogo(FileNameToUpload).Result;
                addUpdateCondimentCategoryMappingRequest.CondimentInfo[0].Icon = imageUrl;

                DataTable dtCondimentNames = Common.ToDataTable(addUpdateCondimentCategoryMappingRequest.CondimentNames);
                DataTable dtCondimentCategories = Common.ToDataTable(addUpdateCondimentCategoryMappingRequest.CondimentCategories);
                DataTable dtCondimentInfo = Common.ToDataTable(addUpdateCondimentCategoryMappingRequest.CondimentInfo);

                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.SL_PT_AddCondimentCategoryMapping, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = CompanyId;
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
                    cmd.Parameters.Add("@CondimentId", SqlDbType.Int).Value = addUpdateCondimentCategoryMappingRequest.CondimentId;

                    var mappingCondimentNames = new SqlParameter("@CondimentNames", SqlDbType.Structured);
                    mappingCondimentNames.TypeName = "dbo.[CondimentName]";
                    mappingCondimentNames.Value = dtCondimentNames;
                    cmd.Parameters.Add(mappingCondimentNames);


                    var mappingCondimentCategories = new SqlParameter("@CondimentCategories", SqlDbType.Structured);
                    mappingCondimentCategories.TypeName = "dbo.[CondimentCategories]";
                    mappingCondimentCategories.Value = dtCondimentCategories;
                    cmd.Parameters.Add(mappingCondimentCategories);


                    var mappingCondimentInfo = new SqlParameter("@CondimentInfo", SqlDbType.Structured);
                    mappingCondimentInfo.TypeName = "dbo.[CondimentInfo]";
                    mappingCondimentInfo.Value = dtCondimentInfo;
                    cmd.Parameters.Add(mappingCondimentInfo);


                    connection.Open();
                    using (SqlDataReader dbReader = cmd.ExecuteReader())
                    {
                        while (dbReader.Read())
                        {
                            isAddedSuccessfully = Convert.ToString(dbReader["SPResult"]) == "1" ? true : false;
                        }
                    }
                }
            }
            catch (Exception ex)  //Logging the Error into file while exception
            {
                Common.WriteLog("PrepTimerController", "AddUpdateCondimentCategoryMapping", ex.Message);
            }
            return isAddedSuccessfully;
        }

        private async Task<string> UpLoadCondimentLogo(string FileNameToUpload)
        {
            string strUrl = string.Empty;
            try
            {
                var companyLogo = HttpContext.Request.Form.Files[0];
                string azureAccountName = Configuration.GetSection("Azure")["AccountName"];
                string azureAccountKey = Configuration.GetSection("Azure")["AccountKey"];

                string fname = "Condiment_Images/" + FileNameToUpload;

                StorageSharedKeyCredential storageCredentials = new StorageSharedKeyCredential(azureAccountName, azureAccountKey);
                Uri blobUri = new Uri("https://" + azureAccountName + ".blob.core.windows.net/" + "storelynk" + "/" + fname);
                BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

                strUrl = "https://" + azureAccountName + ".blob.core.windows.net/" + "storelynk" + "/" + fname;
                using (var stream = new MemoryStream())
                {
                    await companyLogo.CopyToAsync(stream);
                    stream.Position = 0;
                    await blobClient.UploadAsync(stream);
                }
            }
            catch (Exception ex)
            {
                Common.WriteLog("PrepTimerController", "UpLoadCondimentLogo", ex.Message);
            }
            return strUrl;
        }

        private async Task<string> UpLoadFile(string FileNameToUpload)
        {
            string strUrl = string.Empty;
            try
            {
                var companyLogo = HttpContext.Request.Form.Files[0];
                string azureAccountName = Configuration.GetSection("Azure")["AccountName"];
                string azureAccountKey = Configuration.GetSection("Azure")["AccountKey"];


                StorageSharedKeyCredential storageCredentials = new StorageSharedKeyCredential(azureAccountName, azureAccountKey);
                Uri blobUri = new Uri("https://" + azureAccountName + ".blob.core.windows.net/" + "storelynk" + "/" + FileNameToUpload);
                BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

                strUrl = "https://" + azureAccountName + ".blob.core.windows.net/" + "storelynk" + "/" + FileNameToUpload;
                using (var stream = new MemoryStream())
                {
                    await companyLogo.CopyToAsync(stream);
                    stream.Position = 0;
                    await blobClient.UploadAsync(stream);
                }
            }
            catch (Exception ex)
            {
                Common.WriteLog("PrepTimerController", "UpLoadCondimentLogo", ex.Message);
            }
            return strUrl;
        }

        [EnableCors("customPolicy")]
        [Route("GetCondimentCategoryMapping")]
        [HttpPost]
        public List<CondimentCategoriesMappingResponse> GetCondimentCategoryMapping()
        {
            List<CondimentCategoriesMappingResponse> lstCondimentCategoriesMappingResponse = new List<CondimentCategoriesMappingResponse>();
            try
            {
                //CompanyId will get it from header token
                int CompanyId = 0;
                if (!String.IsNullOrEmpty(Request.Headers["CompanyId"]))
                    CompanyId = Convert.ToInt32(Request.Headers["CompanyId"]);

                //UserId will get it from header token
                int UserId = 0;
                if (!String.IsNullOrEmpty(Request.Headers["Userid"]))
                    UserId = Convert.ToInt32(Request.Headers["Userid"]);

                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.SL_PT_GetCondimentCategoryMapping, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = CompanyId;
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
                    cmd.Parameters.Add("@LangName", SqlDbType.NVarChar).Value = "English";


                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            lstCondimentCategoriesMappingResponse = Common.ConvertDataTable<CondimentCategoriesMappingResponse>(dt);
                        }
                    }
                }
            }
            catch (Exception ex)  //Logging the Error into file while exception
            {
                Common.WriteLog("PrepTimerController", "GetCondimentCategoryMapping", ex.Message);
            }
            return lstCondimentCategoriesMappingResponse;
        }


        [EnableCors("customPolicy")]
        [Route("GetLanguageById")]
        [HttpPost]
        public List<LanguagesResponse> GetLanguageById(int LangId)
        {
            List<LanguagesResponse> lstLanguagesResponse = new List<LanguagesResponse>();
            try
            {
                //CompanyId will get it from header token
                int CompanyId = 0;
                if (!String.IsNullOrEmpty(Request.Headers["CompanyId"]))
                    CompanyId = Convert.ToInt32(Request.Headers["CompanyId"]);

                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.PT_GetLanguageById, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@LangId", SqlDbType.Int).Value = LangId;

                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(dt);

                            lstLanguagesResponse = Common.ConvertDataTable<LanguagesResponse>(dt);
                        }
                    }
                }
            }
            catch (Exception ex)  //Logging the Error into file while exception
            {
                Common.WriteLog("PrepTimerController", "GetLanguageById", ex.Message);
            }
            return lstLanguagesResponse;
        }


        [EnableCors("customPolicy")]
        [Route("GetCondimentCategoryById")]
        [HttpPost]
        public CondimentCategoryResponse GetCondimentCategoryById(int CategoryId)
        {
            CondimentCategoryResponse objCondimentCategoryResponse = new CondimentCategoryResponse();
            try
            {
                //CompanyId will get it from header token
                int CompanyId = 0;
                if (!String.IsNullOrEmpty(Request.Headers["CompanyId"]))
                    CompanyId = Convert.ToInt32(Request.Headers["CompanyId"]);

                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.SL_PT_GetCondimentCategoryById, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = CompanyId;
                    cmd.Parameters.Add("@CategoryId", SqlDbType.Int).Value = CategoryId;

                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataSet ds = new DataSet())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(ds);

                            List<CategoryLanguages> lstCategoryLanguages = Common.ConvertDataTable<CategoryLanguages>(ds.Tables[0]);

                            objCondimentCategoryResponse.CategoryId = Convert.ToInt32(ds.Tables[1].Rows[0]["CategoryId"]);
                            objCondimentCategoryResponse.categoryLanguages = lstCategoryLanguages;
                            objCondimentCategoryResponse.StartTime = Convert.ToString(ds.Tables[1].Rows[0]["StartTime"]);
                            objCondimentCategoryResponse.EndTime = Convert.ToString(ds.Tables[1].Rows[0]["EndTime"]);
                            objCondimentCategoryResponse.Status = Convert.ToBoolean(ds.Tables[1].Rows[0]["Status"]);
                        }
                    }
                }
            }
            catch (Exception ex)  //Logging the Error into file while exception
            {
                Common.WriteLog("PrepTimerController", "GetCondimentCategoryById", ex.Message);
            }
            return objCondimentCategoryResponse;
        }


        [EnableCors("customPolicy")]
        [Route("GetCondimentCategoryMappingById")]
        [HttpPost]
        public CondimentCategoryMappingResponse GetCondimentCategoryMappingById(int CondimentId)
        {
            CondimentCategoryMappingResponse objCondimentCategoryMappingResponse = new CondimentCategoryMappingResponse();
            try
            {
                //CompanyId will get it from header token
                int CompanyId = 0;
                if (!String.IsNullOrEmpty(Request.Headers["CompanyId"]))
                    CompanyId = Convert.ToInt32(Request.Headers["CompanyId"]);

                //Getting the connection string from appsettings.json which is included in Starup.cs
                string strSqlConnection = Configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(strSqlConnection))
                {
                    //Calling the stored procedure with all the required paramaters 
                    SqlCommand cmd = new SqlCommand(Keys.SL_PT_GetCondimentCategoryMappingById, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = CompanyId;
                    cmd.Parameters.Add("@CondimentId", SqlDbType.NVarChar).Value = CondimentId;

                    connection.Open();
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataSet ds = new DataSet())
                        {
                            //Moving the SQL response into the DataTable
                            sda.Fill(ds);

                            List<CondimentMappingLanguages> condimentLanguages = Common.ConvertDataTable<CondimentMappingLanguages>(ds.Tables[0]);
                            List<CondimentMappingCategories> condimentCategories = Common.ConvertDataTable<CondimentMappingCategories>(ds.Tables[1]);

                            objCondimentCategoryMappingResponse.CondimentId = Convert.ToInt32(ds.Tables[2].Rows[0]["CondimentId"]);
                            objCondimentCategoryMappingResponse.condimentLanguages = condimentLanguages;
                            objCondimentCategoryMappingResponse.condimentCategories = condimentCategories;
                            objCondimentCategoryMappingResponse.Icon = Convert.ToString(ds.Tables[2].Rows[0]["Icon"]);
                            objCondimentCategoryMappingResponse.TimePeriod = Convert.ToInt32(ds.Tables[2].Rows[0]["TimePeriod"]);
                            objCondimentCategoryMappingResponse.LineNumber = Convert.ToInt32(ds.Tables[2].Rows[0]["LineNumber"]);
                            objCondimentCategoryMappingResponse.Status = Convert.ToBoolean(ds.Tables[2].Rows[0]["Status"]);
                            objCondimentCategoryMappingResponse.IsEssential = Convert.ToBoolean(ds.Tables[2].Rows[0]["IsEssential"]);
                        }
                    }
                }
            }
            catch (Exception ex)  //Logging the Error into file while exception
            {
                Common.WriteLog("PrepTimerController", "GetCondimentCategoryMappingById", ex.Message);
            }
            return objCondimentCategoryMappingResponse;
        }
    }
}
