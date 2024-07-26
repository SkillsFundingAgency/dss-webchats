using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NCS.DSS.WebChat.Helpers
{
    public static class HttpResponseMessageHelper
    {

        #region Ok(200)

        public static IActionResult Ok(Guid id)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(id), 
                    Encoding.UTF8, "application/json")
            };
        }

        public static IActionResult Ok(string resourceJson)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(resourceJson, Encoding.UTF8, "application/json")
            };
        }

        #endregion

        #region Created(201) 

        public static IActionResult Created(string resourceJson)
        {
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(resourceJson, Encoding.UTF8, "application/json")
            };
        }

        #endregion

        #region NoContent(204)

        public static IActionResult NoContent(Guid id)
        {
            return new HttpResponseMessage(HttpStatusCode.NoContent)
            {
                Content = new StringContent(JsonConvert.SerializeObject(id), 
                    Encoding.UTF8, "application/json")
            };

        }

        #endregion

        #region BadRequest(400)

        public static IActionResult BadRequest()
        {
           return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        public static IActionResult BadRequest(Guid id)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(JsonConvert.SerializeObject(id), 
                    Encoding.UTF8, "application/json")
            };
        }

        #endregion

        #region Forbidden(403)

        public static IActionResult Forbidden(Guid id)
        {
            return new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                Content = new StringContent(JsonConvert.SerializeObject(id),
                    Encoding.UTF8, "application/json")
            };
        }

        #endregion

        #region UnprocessableEntity(422)

        public static IActionResult UnprocessableEntity(HttpRequest req)
        {
            return new HttpResponseMessage((HttpStatusCode)422)
            {
                Content = new StringContent(JsonConvert.SerializeObject(req),
                    Encoding.UTF8, "application/json")
            };
        }

        public static IActionResult UnprocessableEntity(List<ValidationResult> errors)
        {
            return new HttpResponseMessage((HttpStatusCode)422)
            {
                Content = new StringContent(JsonConvert.SerializeObject(errors),
                    Encoding.UTF8, "application/json")
            };
        }

        public static IActionResult UnprocessableEntity(JsonException requestException)
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
