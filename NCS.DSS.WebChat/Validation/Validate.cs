using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.WebChat.Models;

namespace NCS.DSS.WebChat.Validation
{
    public class Validate : IValidate
    {
        public List<ValidationResult> ValidateResource(IWebChat resource, bool validateModelForPost)
        {
            var context = new ValidationContext(resource, null, null);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(resource, context, results, true);

            ValidateWebChatRules(resource, results, validateModelForPost);

            return results;
        }

        private void ValidateWebChatRules(IWebChat webChatResource, List<ValidationResult> results, bool validateModelForPost)
        {
            if (webChatResource == null)
                return;

            if (validateModelForPost)
            {
                if (string.IsNullOrWhiteSpace(webChatResource.WebChatNarrative))
                    results.Add(new ValidationResult("Web Chat Narrative is a required field", new[] { "WebChatNarrative" }));
            }

            if (webChatResource.WebChatStartDateandTime.HasValue && webChatResource.WebChatStartDateandTime.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Web Chat Start Date and Time must be less the current date/time", new[] { "WebChatStartDateandTime" }));

            if (webChatResource.WebChatEndDateandTime.HasValue && webChatResource.WebChatEndDateandTime.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Web Chat End Date and Time must be less the current date/time", new[] { "WebChatEndDateandTime" }));

            if (webChatResource.DateandTimeSentToCustomers.HasValue && webChatResource.DateandTimeSentToCustomers.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Date and Time Sent To Customers must be less the current date/time", new[] { "DateandTimeSentToCustomers" }));

            if (webChatResource.LastModifiedDate.HasValue && webChatResource.LastModifiedDate.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Last Modified Date must be less the current date/time", new[] { "LastModifiedDate" }));

        }

    }
}
