using DFC.Swagger.Standard;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Mvc;

namespace NCS.DSS.WebChat.APIDefinition
{
    public class ApiDefinition
    {
        public const string APITitle = "WebChats";
        public const string APIDefinitionName = "API-Definition";
        public const string APIDefRoute = APITitle + "/" + APIDefinitionName;
        public const string APIDescription = "Basic details of a National Careers Service " + APITitle + " Resource";
        public const string ApiVersion = "1.0.0";
        private ISwaggerDocumentGenerator swaggerDocumentGenerator;

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
                return new HttpResponseMessage(HttpStatusCode.NoContent);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(swagger)
            };
        }
    }
}