﻿using DFC.Swagger.Standard.Annotations;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.WebChat.Models
{
    public class WebChat : IWebChat
    {
        [Display(Description = "Unique identifier of the web chat record.")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public Guid? WebChatId { get; set; }

        [Display(Description = "Unique identifier of a customer.")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        public Guid? CustomerId { get; set; }

        [Display(Description = "Unique identifier of the customer interaction record.")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        public Guid? InteractionId { get; set; }

        [StringLength(100)]
        [RegularExpression(@"^[0-9a-zA-Z\.]+$")]
        [Display(Description = "Unique identifier passed from the Digital Service to the webchat session.")]
        [Example(Description = "abc123")]
        public string DigitalReference { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the webchat webchat was initiated by the customer.")]
        [Example(Description = "2018-06-20T13:20:00")]
        public DateTime? WebChatStartDateandTime { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the webchat with the webchat finished or was terminated by the customer.")]
        [Example(Description = "2018-06-20T13:45:00")]
        public DateTime? WebChatEndDateandTime { get; set; }

        [DataType(DataType.Time)]
        [Display(Description = "Duration in h:m:s of the webchat conversation.")]
        [Example(Description = "01:41:19")]
        public TimeSpan? WebChatDuration { get; set; }

        [Required]
        [StringLength(100000)]
        [RegularExpression(@"^[^<>]+$")]
        [Display(Description = "Webchat text.")]
        [Example(Description = "this is some text")]
        public string WebChatNarrative { get; set; }

        [Display(Description = "Indicator to say whether the web chat text has been sent to the customer.")]
        [Example(Description = "true")]
        public bool? SentToCustomer { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the web chat narrative was sent to the customer.")]
        [Example(Description = "2018-06-20T13:45:00")]
        public DateTime? DateandTimeSentToCustomers { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        [Example(Description = "2018-06-20T13:45:00")]
        public DateTime? LastModifiedDate { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^[0-9]+$")]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string LastModifiedTouchpointId { get; set; }

        public void SetDefaultValues()
        {
            if (!LastModifiedDate.HasValue)
                LastModifiedDate = DateTime.UtcNow;

            if (WebChatStartDateandTime.HasValue && WebChatEndDateandTime.HasValue)
                WebChatDuration = WebChatEndDateandTime.Value.Subtract(WebChatStartDateandTime.Value);
        }

        public void SetIds(Guid customerId, Guid interactionId, string touchpointId)
        {
            WebChatId = Guid.NewGuid();
            CustomerId = customerId;
            InteractionId = interactionId;
            LastModifiedTouchpointId = touchpointId;
        }

        public void Patch(WebChatPatch webChatPatch)
        {
            if (webChatPatch == null)
                return;

            if (!string.IsNullOrEmpty(webChatPatch.DigitalReference))
                DigitalReference = webChatPatch.DigitalReference;

            if (webChatPatch.WebChatStartDateandTime.HasValue)
                WebChatStartDateandTime = webChatPatch.WebChatStartDateandTime;

            if (webChatPatch.WebChatEndDateandTime.HasValue)
                WebChatEndDateandTime = webChatPatch.WebChatEndDateandTime;

            if (webChatPatch.WebChatDuration.HasValue)
                WebChatDuration = webChatPatch.WebChatDuration;

            if (!string.IsNullOrEmpty(webChatPatch.WebChatNarrative))
                WebChatNarrative = webChatPatch.WebChatNarrative;

            if (webChatPatch.SentToCustomer.HasValue)
                SentToCustomer = webChatPatch.SentToCustomer;

            if (webChatPatch.DateandTimeSentToCustomers.HasValue)
                DateandTimeSentToCustomers = webChatPatch.DateandTimeSentToCustomers;

            if (webChatPatch.LastModifiedDate.HasValue)
                LastModifiedDate = webChatPatch.LastModifiedDate;

            if (!string.IsNullOrEmpty(webChatPatch.LastModifiedTouchpointId))
                LastModifiedTouchpointId = webChatPatch.LastModifiedTouchpointId;
        }
    }
}