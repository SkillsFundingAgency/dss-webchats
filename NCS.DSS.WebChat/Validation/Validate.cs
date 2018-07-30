using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.WebChat.Models;

namespace NCS.DSS.WebChat.Validation
{
    public class Validate : IValidate
    {
        public List<ValidationResult> ValidateResource(IWebChat resource)
        {
            var context = new ValidationContext(resource, null, null);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(resource, context, results, true);

            ValidateWebChatRules(resource, results);

            return results;
        }

        private void ValidateWebChatRules(IWebChat webChatResource, List<ValidationResult> results)
        {
            if (webChatResource == null)
                return;

            if (webChatResource.WebChatStartDateandTime.HasValue && webChatResource.WebChatStartDateandTime.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Web Chat Start Date and Time must be less the current date/time", new[] { "WebChatStartDateandTime" }));

            if (webChatResource.WebChatEndDateandTime.HasValue && webChatResource.WebChatEndDateandTime.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Web Chat End Date and Time must be less the current date/time", new[] { "WebChatEndDateandTime" }));

            if (webChatResource.DateandTimeSentToCustomers.HasValue && webChatResource.DateandTimeSentToCustomers.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Date and Time Sent To Customers must be less the current date/time", new[] { "DateandTimeSentToCustomers" }));

            if (webChatResource.LastModifiedDate.HasValue && webChatResource.LastModifiedDate.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Last Modified Date must be less the current date/time", new[] { "LastModifiedDate" }));

            if (string.IsNullOrWhiteSpace(webChatResource.WebChatNarrative))
                results.Add(new ValidationResult("Web Chat Narrative is a required field", new[] { "WebChatNarrative" }));


        }

    }
}
