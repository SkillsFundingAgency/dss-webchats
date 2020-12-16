using DFC.Swagger.Standard.Annotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.WebChat.Models
{
    public class WebChatPatch : IWebChat
    {
        [StringLength(100)]
        [Display(Description = "Unique identifier passed from the Digital Service to the webchat webchat.")]
        [Example(Description = "abc123")]
        public string DigitalReference { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the webchat webchat was initiated by the customer.")]
        [Example(Description = "2018-06-20T13:20:00")]
        public DateTime? WebChatStartDateandTime { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the webchat with the webchat finished or was terminated by the customer.")]
        [Example(Description = "2018-06-20T13:45:00")]
        public DateTime? WebChatEndDateandTime { get; set; }

        [DataType(DataType.Time)]
        [Display(Description = "Duration in h:m:s of the webchat conversation.")]
        [Example(Description = "01:41:19")]
        public TimeSpan? WebChatDuration { get; set; }

        [StringLength(100000)]
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
    }
}