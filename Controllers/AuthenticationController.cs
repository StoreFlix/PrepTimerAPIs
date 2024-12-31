using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using ServiceFabricAPIsOld.Model;
using Microsoft.Extensions.Configuration;
using ServiceFabricApp.API.Model;
using ServiceFabricApp.API.Repositories;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Data;

namespace ServiceFabricAPIsOld.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {


        /// <summary>
        /// Configuration
        /// </summary>
        private readonly IConfiguration Configuration;


        /// <summary>
        /// CustomerController
        /// </summary>
        /// <param name="_configuration"></param>
        public AuthenticationController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }


       
    }
}
