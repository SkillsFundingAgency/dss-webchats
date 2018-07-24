﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace NCS.DSS.WebChat.Helpers
{
    public static class HttpResponseMessageHelper
    {

        #region Ok(200)

        public static HttpResponseMessage Ok(Guid id)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(id), 
                    Encoding.UTF8, "application/json")
            };
        }

        public static HttpResponseMessage Ok<T>(T resource)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(resource), 
                    Encoding.UTF8, "application/json")
            };
        }

        public static HttpResponseMessage Ok<T>(List<T> resourcesList)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(resourcesList),
                    Encoding.UTF8, "application/json")
            };
        }

        #endregion

        #region Created(201) 

        public static HttpResponseMessage Created<T>( T resource)
        {
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(JsonConvert.SerializeObject(resource), 
                    Encoding.UTF8, "application/json")
            };
        }

        #endregion

        #region NoContent(204)

        public static HttpResponseMessage NoContent(Guid id)
        {
            return new HttpResponseMessage(HttpStatusCode.NoContent)
            {
                Content = new StringContent(JsonConvert.SerializeObject(id), 
                    Encoding.UTF8, "application/json")
            };

        }

        #endregion

        #region BadRequest(400)

        public static HttpResponseMessage BadRequest(Guid id)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(JsonConvert.SerializeObject(id), 
                    Encoding.UTF8, "application/json")
            };
        }

        #endregion

        #region UnprocessableEntity(422)

        public static HttpResponseMessage UnprocessableEntity(HttpRequestMessage req)
        {
            return new HttpResponseMessage((HttpStatusCode)422)
            {
                Content = new StringContent(JsonConvert.SerializeObject(req),
                    Encoding.UTF8, "application/json")
            };
        }

        public static HttpResponseMessage UnprocessableEntity(List<ValidationResult> errors)
        {
            return new HttpResponseMessage((HttpStatusCode)422)
            {
                Content = new StringContent(JsonConvert.SerializeObject(errors),
                    Encoding.UTF8, "application/json")
            };
        }

        public static HttpResponseMessage UnprocessableEntity(JsonException requestException)
        {
            return new HttpResponseMessage((HttpStatusCode)422)
            {
                Content = new StringContent(JsonConvert.SerializeObject(requestException),
                    Encoding.UTF8, "application/json")
            };
        }


        #endregion

    }
}
