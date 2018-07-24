using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCS.DSS.WebChat.GetWebChatByIdHttpTrigger.Service
{
    public class GetWebChatByIdHttpTriggerService : IGetWebChatByIdHttpTriggerService
    {
        public async Task<Models.WebChat> GetWebChat(Guid webChatId)
        {
            var webChats = CreateTempWebChats();
            var result = webChats.FirstOrDefault(a => a.WebChatId == webChatId);
            return await Task.FromResult(result);
        }

        public List<Models.WebChat> CreateTempWebChats()
        {
            var webChatList = new List<Models.WebChat>
            {
                new Models.WebChat
                {
                    WebChatId = Guid.Parse("218885cd-d735-4220-8d53-67acbc6eb971"),
                    InteractionId = Guid.NewGuid(),
                    WebChatStartDateandTime = DateTime.UtcNow,
                    WebChatEndDateandTime = DateTime.UtcNow.AddMinutes(34),
                    WebChatDuration = new TimeSpan(0,0,34),
                    WebChatNarrative = "Hi My Name is.....",
                    SentToCustomer = true,
                    LastModifiedDate = DateTime.Today,
                    LastModifiedTouchpointId = Guid.NewGuid()
                },
                new Models.WebChat
                {
                    WebChatId = Guid.Parse("f2f29384-6b0d-483b-b813-1a5520ce54c2"),
                    InteractionId = Guid.NewGuid(),
                    WebChatStartDateandTime = DateTime.UtcNow,
                    WebChatEndDateandTime = DateTime.UtcNow.AddHours(1),
                    WebChatDuration = new TimeSpan(0,1,0),
                    WebChatNarrative = "Hi My Name is....., I would like to know.....",
                    SentToCustomer = false,
                    LastModifiedDate = DateTime.Today,
                    LastModifiedTouchpointId = Guid.NewGuid()
                },
                new Models.WebChat
                {
                    WebChatId = Guid.Parse("bd654883-c5d6-48da-8fe2-710c0452f016"),
                    InteractionId = Guid.NewGuid(),
                    WebChatStartDateandTime = DateTime.UtcNow,
                    WebChatEndDateandTime = DateTime.UtcNow.AddDays(1),
                    WebChatDuration = new TimeSpan(1,0,0),
                    WebChatNarrative = "Test 3",
                    SentToCustomer = true,
                    LastModifiedDate = DateTime.Today,
                    LastModifiedTouchpointId = Guid.NewGuid()
                }
            };

            return webChatList;
        }
    }
}