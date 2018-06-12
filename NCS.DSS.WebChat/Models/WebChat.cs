using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.WebChat.Models
{
    public class WebChat
    {
        public Guid WebChatId { get; set; }

        [Required]
        public Guid InteractionId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime WebChatStartDateandTime { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime WebChatEndDateandTime { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan WebChatDuration { get; set; }

        [Required]
        public string WebChatNarrative { get; set; }

        public bool SentToCustomer { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateandTimeSentToCustomers { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LastModifiedDate { get; set; }

        public Guid LastModifiedTouchpointId { get; set; }
    }
}