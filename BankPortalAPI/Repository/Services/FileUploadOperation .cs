using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BankPortalAPI.Repository.Services
{
    //public class FileUploadOperation : IOperationFilter
    //{
    //    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    //    {
    //        var fileParams = context.MethodInfo
    //            .GetParameters()
    //            .Where(p => p.ParameterType == typeof(IFormFile));

    //        if (fileParams.Any())
    //        {
    //            operation.RequestBody = new OpenApiRequestBody
    //            {
    //                Content = new Dictionary<string, OpenApiMediaType>
    //                {
    //                    ["multipart/form-data"] = new OpenApiMediaType
    //                    {
    //                        Schema = new OpenApiSchema
    //                        {
    //                            Type = "object",
    //                            Properties = new Dictionary<string, OpenApiSchema>
    //                            {
    //                                ["file"] = new OpenApiSchema
    //                                {
    //                                    Type = "string",
    //                                    Format = "binary"
    //                                }
    //                            },
    //                            Required = new HashSet<string> { "file" }
    //                        }
    //                    }
    //                }
    //            };
    //        }
    //    }
    //}
}
