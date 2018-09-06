﻿using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NCS.DSS.WebChat.Helpers
{
    public class HttpRequestMessageHelper : IHttpRequestMessageHelper
    {
        public async Task<T> GetWebChatFromRequest<T>(HttpRequestMessage req)
        {
            if (req == null)
                return default(T);

            if (req.Content?.Headers != null)
                req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return await req.Content.ReadAsAsync<T>();
        }

        public string GetTouchpointId(HttpRequestMessage req)
        {
            if (req?.Headers == null)
                return null;

            if (!req.Headers.Contains("TouchpointId"))
                return null;

            var touchpointId = req.Headers.GetValues("TouchpointId").FirstOrDefault();

            return string.IsNullOrEmpty(touchpointId) ? string.Empty : touchpointId;
        }

        public string GetApimURL(HttpRequestMessage req)
        {
            if (req?.Headers == null)
                return null;

            if (!req.Headers.Contains("apimurl"))
                return null;

            var ApimURL = req.Headers.GetValues("apimurl").FirstOrDefault();

            return string.IsNullOrEmpty(ApimURL) ? string.Empty : ApimURL;
        }

    }
}
