using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NCS.DSS.WebChat.Helpers
{
    public class HttpRequestMessageHelper : IHttpRequestMessageHelper
    {
        public async Task<T> GetWebChatFromRequest<T>(HttpRequest req)
        {
            if (req == null)
                return default(T);

            if (req.Content?.Headers != null)
                req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return await req.Content.ReadAsAsync<T>();
        }

        public string GetTouchpointId(HttpRequest req)
        {
            if (req?.Headers == null)
                return null;

            if (!req.Headers.Contains("TouchpointId"))
                return null;

            var touchpointId = req.Headers.GetValues("TouchpointId").FirstOrDefault();

            return string.IsNullOrEmpty(touchpointId) ? string.Empty : touchpointId;
        }

        public string GetApimURL(HttpRequest req)
        {
            if (req?.Headers == null)
                return null;

            if (!req.Headers.Contains("apimurl"))
                return null;

            var ApimURL = req.Headers.GetValues("apimurl").FirstOrDefault();

            if (ApimURL.EndsWith("/"))
                ApimURL = ApimURL.Substring(0, ApimURL.Length - 1);


            return string.IsNullOrEmpty(ApimURL) ? string.Empty : ApimURL;
        }

    }
}
