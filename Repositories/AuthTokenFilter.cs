using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace ServiceFabricApp.API
{
    /// <summary>
    /// Operation filter to add the requirement of the custom header
    /// </summary>
    public class AuthTokenFilter : IOperationFilter
    {
        /// <summary>
        /// Apply
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            //Header added for token to send it from all the API's which is going to be a mandetory
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "token",
                In = ParameterLocation.Header,
                Required = false 
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "code",
                In = ParameterLocation.Header,
                Required = false
            });
        }
    }
}
