using System;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.WebChat.Cosmos.Helper;
using NCS.DSS.WebChat.GetWebChatByIdHttpTrigger.Service;
using NCS.DSS.WebChat.GetWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.Helpers;
using NCS.DSS.WebChat.PatchWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.PostWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.Validation;


namespace NCS.DSS.WebChat.Ioc
{
    public class RegisterServiceProvider
    {
        public IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddTransient<IGetWebChatHttpTriggerService, GetWebChatHttpTriggerService>();
            services.AddTransient<IGetWebChatByIdHttpTriggerService, GetWebChatByIdHttpTriggerService>();
            services.AddTransient<IPostWebChatHttpTriggerService, PostWebChatHttpTriggerService>();
            services.AddTransient<IPatchWebChatHttpTriggerService, PatchWebChatHttpTriggerService>();
            services.AddTransient<IResourceHelper, ResourceHelper>();
            services.AddTransient<IValidate, Validate>();
            services.AddTransient<IHttpRequestMessageHelper, HttpRequestMessageHelper>();
            return services.BuildServiceProvider(true);
        }
    }
}
