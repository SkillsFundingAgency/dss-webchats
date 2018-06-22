using System;

namespace NCS.DSS.WebChat.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class Response : Attribute
    {
        public int HttpStatusCode { get; set; }
        public string Description { get; set; }
        public bool ShowSchema { get; set; }
    }
}
