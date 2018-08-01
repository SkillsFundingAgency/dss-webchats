using System;

namespace NCS.DSS.WebChat.Models
{
    public interface IWebChat
    {
        string DigitalReference { get; set; }
        DateTime? WebChatStartDateandTime { get; set; }
        DateTime? WebChatEndDateandTime { get; set; }
        TimeSpan? WebChatDuration { get; set; }
        string WebChatNarrative { get; set; }
        bool? SentToCustomer { get; set; }
        DateTime? DateandTimeSentToCustomers { get; set; }
        DateTime? LastModifiedDate { get; set; }
        Guid? LastModifiedTouchpointId { get; set; }

        void SetDefaultValues();
    }
}