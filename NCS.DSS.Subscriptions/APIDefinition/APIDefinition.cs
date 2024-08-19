using DFC.Swagger.Standard;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Mvc;

namespace NCS.DSS.Subscriptions.APIDefinition
{
    public class ApiDefinition
    {
        public const string APITitle = "Subscriptions";
        public const string APIDefinitionName = "API-Definition";
        public const string APIDefRoute = APITitle + "/" + APIDefinitionName;
        public const string APIDescription = "Basic details of a National Careers Service " + APITitle + " Resource";
        private readonly ISwaggerDocumentGenerator swaggerDocumentGenerator;
        private string ApiVersion = "2.0.0";


        public ApiDefinition(ISwaggerDocumentGenerator swaggerDocumentGenerator)
        {
            this.swaggerDocumentGenerator = swaggerDocumentGenerator;
        }

        [Function(APIDefinitionName)]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = APIDefRoute)] HttpRequest req)
        {
            var swagger = swaggerDocumentGenerator.GenerateSwaggerDocument(req, APITitle, APIDescription,
                APIDefinitionName, ApiVersion, Assembly.GetExecutingAssembly());

            if (string.IsNullOrEmpty(swagger))
                return new NoContentResult();

            return new OkObjectResult(swagger);
        }
    }
}

