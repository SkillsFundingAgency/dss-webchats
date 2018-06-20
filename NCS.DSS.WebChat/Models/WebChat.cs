using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.WebChat.Models
{
    public class WebChat
    {

        [Display(Description = "Unique identifier of the web chat record.")]
        public Guid WebChatId { get; set; }

        [Required]
        [Display(Description = "Unique identifier of the customer interaction record.")]
        public Guid InteractionId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the webchat session was initiated by the customer.")]
        public DateTime WebChatStartDateandTime { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the webchat with the session finished or was terminated by the customer.")]
        public DateTime WebChatEndDateandTime { get; set; }

        [DataType(DataType.Time)]
        [Display(Description = "Duration in h:m:s of the webchat conversation.")]
        public TimeSpan WebChatDuration { get; set; }

        [Required]
        [StringLength(10000)]
        [Display(Description = "Webchat text.")]
        public string WebChatNarrative { get; set; }

        [Display(Description = "Indicator to say whether the web chat text has been sent to the customer.")]
        public bool SentToCustomer { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the web chat narrative was sent to the customer.")]
        public DateTime DateandTimeSentToCustomers { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        public DateTime LastModifiedDate { get; set; }

        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        public Guid LastModifiedTouchpointId { get; set; }
    }
}