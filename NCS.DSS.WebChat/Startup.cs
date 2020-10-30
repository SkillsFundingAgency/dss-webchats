﻿using DFC.Swagger.Standard;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.WebChat;
using NCS.DSS.WebChat.Cosmos.Helper;
using NCS.DSS.WebChat.GetWebChatByIdHttpTrigger.Service;
using NCS.DSS.WebChat.GetWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.Helpers;
using NCS.DSS.WebChat.PatchWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.PostWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.Validation;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: FunctionsStartup(typeof(Startup))]
namespace NCS.DSS.WebChat
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
            builder.Services.AddTransient<IGetWebChatHttpTriggerService, GetWebChatHttpTriggerService>();
            builder.Services.AddTransient<IGetWebChatByIdHttpTriggerService, GetWebChatByIdHttpTriggerService>();
            builder.Services.AddTransient<IPostWebChatHttpTriggerService, PostWebChatHttpTriggerService>();
            builder.Services.AddTransient<IPatchWebChatHttpTriggerService, PatchWebChatHttpTriggerService>();
            builder.Services.AddTransient<IResourceHelper, ResourceHelper>();
            builder.Services.AddTransient<IValidate, Validate>();
            builder.Services.AddTransient<IHttpRequestMessageHelper, HttpRequestMessageHelper>();
        }
    }
}


//services.AddTransient<IGetWebChatHttpTriggerService, GetWebChatHttpTriggerService>();
//services.AddTransient<IGetWebChatByIdHttpTriggerService, GetWebChatByIdHttpTriggerService>();
//services.AddTransient<IPostWebChatHttpTriggerService, PostWebChatHttpTriggerService>();
//services.AddTransient<IPatchWebChatHttpTriggerService, PatchWebChatHttpTriggerService>();
//services.AddTransient<IResourceHelper, ResourceHelper>();
//services.AddTransient<IValidate, Validate>();
//services.AddTransient<IHttpRequestMessageHelper, HttpRequestMessageHelper>();