﻿using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.WebChat.Helpers
{
    public interface IHttpRequestMessageHelper
    {
        Task<T> GetWebChatFromRequest<T>(HttpRequestMessage req);
        string GetTouchpointId(HttpRequestMessage req);
        string GetApimURL(HttpRequestMessage req);
    }
}