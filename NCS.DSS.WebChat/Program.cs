using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCS.DSS.WebChat.Cosmos.Helper;
using NCS.DSS.WebChat.GetWebChatByIdHttpTrigger.Service;
using NCS.DSS.WebChat.GetWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.Helpers;
using NCS.DSS.WebChat.PatchWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.PostWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.Validation;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddLogging();
        services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
        services.AddTransient<IGetWebChatHttpTriggerService, GetWebChatHttpTriggerService>();
        services.AddTransient<IGetWebChatByIdHttpTriggerService, GetWebChatByIdHttpTriggerService>();
        services.AddTransient<IPostWebChatHttpTriggerService, PostWebChatHttpTriggerService>();
        services.AddTransient<IPatchWebChatHttpTriggerService, PatchWebChatHttpTriggerService>();
        services.AddTransient<IResourceHelper, ResourceHelper>();
        services.AddTransient<IValidate, Validate>();
        services.AddTransient<IHttpRequestHelper, HttpRequestHelper>();
        services.AddTransient<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
        services.AddTransient<IJsonHelper, DFC.JSON.Standard.JsonHelper>();
        services.AddSingleton<IDynamicHelper, DynamicHelper>();
    })
    .Build();

host.Run();
