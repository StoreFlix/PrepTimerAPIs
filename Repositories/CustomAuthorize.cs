using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Web.Http;
using Jot;
//using System.Web.Script.Serialization;
using System.Text;
using ServiceFabricApp.API.Model;
using System.Configuration;
using Newtonsoft.Json;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ServiceFabricApp.API.Repositories
{
    //public class CustomIsAuthorized : System.Web.Http.AuthorizeAttribute
    //public class CustomOnauthorizationAttribute : System.Web.Http.AuthorizeAttribute

    [AttributeUsage(AttributeTargets.Method)]
    public  class CustomOnauthorizationAttribute : System.Web.Http.AuthorizeAttribute, IFilterMetadata


    {
        private static string errMessage = string.Empty;

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Forbidden,
                Content = new StringContent(errMessage)
            };
        }
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            bool authorized = false;
             
        //  var t = actionContext.Request.Headers.GetValues("Token");
            string authModel = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE1OTI1NzkwNzUsImV4cCI6MTU5Mzg3NTA3NSwianRpIjoiMzM4NmQwZjQtNzE2OC00MDM2LWE1ZDktNTVlNGNlODlmNTc4IiwiaXNzIjoiIiwiYXVkIjoiIiwibmJmIjoxNTkyNTc5MDc1LCJzdWIiOiIiLCJ1c2VybmFtZSI6ImNsaWZ0b25za3lsaW5lQGZ1c2UubmV0IiwiQXV0aFR5cGUiOiJVIn0.GF3m6MEsIsLU39gfO-67SDr1PKpkBg-FLnxHyhHKErY";
            //string headerToken = actionContext.Request.Headers.Authorization ;
            try
            {
                actionContext.Request.Headers.Add("CompanyId", "123");
               // var id = headerValues.FirstOrDefault();
                //var validatedResponse = TokenManager.DecodeAndValidateToken();
                int duration = 0;
                //if (authModel.username != null)
                //{
                var deserializedToken = new TokenManager();
                var decodeToken = deserializedToken.DecodeAPI(authModel);

                // var decodeToken = TokenManager.DecodeAndValidateToken(authModel);
                // var validatedResponse11 = TokenManager.Decode(authModel);
                //  var decodeToken = deserializedToken.Decode(authModel.username);
                var tickValue = Convert.ToInt64(decodeToken[0]);
                var tickDuration = tickValue / 60;
                duration = Convert.ToInt32(tickDuration);

                // }
                var validateResult = new TokenManager().ValidateToken(authModel, duration);
                if (validateResult.ToString() == "Passed")
                {
                    //If (decodeToken[2] =="U")
                    //   string RegionIds = actionContext.Headers.GetValues("RegionIds");
                    //   string TerritoryIds = actionContext.Headers.GetValues("TerritoryIds");
                    //   string SubTerritoryIds = actionContext.Headers.GetValues("SubTerritoryIds");
                    var RegionIds = "289";
                    var TerritoryIds = "1760";
                    var SubTerritoryIds = "13010";
                      var Authcnt = new TokenManager();
                    // var Authcnt1 = Authcnt.ValidateUser(decodeToken[1]);
                    var Authcnt1 = Authcnt.ValidateUser(decodeToken[1], RegionIds, TerritoryIds, SubTerritoryIds);
                    Authcnt1 = "0";

                    if (Authcnt1 == "0")
                    {
                        //HttpContext.Current.Response.AddHeader("authenticationToken", authModel);
                        //HttpContext.Current.Response.AddHeader("AuthenticationStatus", "Authorized");
                        //actionContext.Response = new HttpResponseMessage((HttpStatusCode)200) { ReasonPhrase = "Authorized user" };
                        ////   actionContext.HttpContext.Response.Redirect(Url.Action("AdminLogin", "values"));
                        //base.OnAuthorization(actionContext);  //  return;

                        authorized = true;
                        // message.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        //HttpContext.Current.Response.AddHeader("authenticationToken", authModel);
                        //HttpContext.Current.Response.AddHeader("AuthenticationStatus", "NotAuthorized");
                        //return; //message.CreateResponse(HttpStatusCode.Unauthorized);
                        errMessage = "Access Denied";
                    }
                }
                else
                {
                        //HttpContext.Current.Response.AddHeader("authenticationToken", authModel);
                        //HttpContext.Current.Response.AddHeader("AuthenticationStatus", "NotAuthorized");
                        //return;// message.CreateResponse(HttpStatusCode.Unauthorized);
                        errMessage = "Not Authorised";
                }
        
            }
            catch (Exception ex)
            {
                errMessage = "An error has occurred while authanticating user.";
                //return;//message.CreateResponse(HttpStatusCode.Unauthorized);
            }

            return authorized;
        }
    }
}

