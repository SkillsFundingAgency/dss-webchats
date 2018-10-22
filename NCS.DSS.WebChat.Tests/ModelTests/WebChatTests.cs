using System;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.WebChat.Tests.ModelTests
{
    [TestFixture]
    public class WebChatTests
    {

        [Test]
        public void WebChatTests_PopulatesDefaultValues_WhenSetDefaultValuesIsCalled()
        {
            var webChat = new Models.WebChat();
            webChat.SetDefaultValues();

            // Assert
            Assert.IsNotNull(webChat.LastModifiedDate);
        }

        [Test]
        public void WebChatTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var webChat = new Models.WebChat { LastModifiedDate = DateTime.MaxValue };

            webChat.SetDefaultValues();

            // Assert
            Assert.AreEqual(DateTime.MaxValue, webChat.LastModifiedDate);
        }
        
        [Test]
        public void WebChatTests_CheckWebChatDurationDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var webChat = new Models.WebChat { WebChatStartDateandTime = DateTime.UtcNow };

            webChat.SetDefaultValues();

            // Assert
            Assert.IsNull(webChat.WebChatDuration);
        }

        [Test]
        public void WebChatTests_CheckWebChatEndDateandTimeDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var webChat = new Models.WebChat { WebChatEndDateandTime = DateTime.UtcNow };

            webChat.SetDefaultValues();

            // Assert
            Assert.IsNull(webChat.WebChatDuration);
        }

        [Test]
        public void WebChatTests_CheckWebChatDurationGetsPopulated_WhenSetDefaultValuesIsCalled()
        {
            var webChat = new Models.WebChat { WebChatStartDateandTime = DateTime.UtcNow.AddHours(-2),
                WebChatEndDateandTime = DateTime.UtcNow };

            webChat.SetDefaultValues();

            // Assert
            Assert.IsNotEmpty(webChat.WebChatDuration.ToString());
        }

        [Test]
        public void WebChatTests_CheckWebChatIdIsSet_WhenSetIdsIsCalled()
        {
            var webChat = new Models.WebChat();

            webChat.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>());

            // Assert
            Assert.AreNotSame(Guid.Empty, webChat.WebChatId);
        }

        [Test]
        public void WebChatTests_CheckCustomerIdIsSet_WhenSetIdsIsCalled()
        {
            var webChat = new Models.WebChat();

            var customerId = Guid.NewGuid();
            webChat.SetIds(customerId, Arg.Any<Guid>(), Arg.Any<string>());

            // Assert
            Assert.AreEqual(customerId, webChat.CustomerId);
        }

        [Test]
        public void WebChatTests_CheckInteractionIdIsSet_WhenSetIdsIsCalled()
        {
            var webChat = new Models.WebChat();

            var interactionId = Guid.NewGuid();
            webChat.SetIds(Arg.Any<Guid>(), interactionId, Arg.Any<string>());

            // Assert
            Assert.AreEqual(interactionId, webChat.InteractionId);
        }

        [Test]
        public void WebChatTests_CheckLastModifiedTouchpointIdIsSet_WhenSetIdsIsCalled()
        {
            var webChat = new Models.WebChat();

            webChat.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(), "0000000000");

            // Assert
            Assert.AreEqual("0000000000", webChat.LastModifiedTouchpointId);
        }

        [Test]
        public void WebChatTests_CheckDigitalReferenceIsUpdated_WhenPatchIsCalled()
        {
            var webChat = new Models.WebChat { DigitalReference = "Empty" };
            var webChatPatch = new Models.WebChatPatch { DigitalReference = "DigitalReference" };

            webChat.Patch(webChatPatch);

            // Assert
            Assert.AreEqual("DigitalReference", webChat.DigitalReference);
        }

        [Test]
        public void WebChatTests_CheckWebChatStartDateandTimeIsUpdated_WhenPatchIsCalled()
        {
            var webChat = new Models.WebChat { WebChatStartDateandTime = DateTime.UtcNow};
            var webChatPatch = new Models.WebChatPatch { WebChatStartDateandTime = DateTime.MaxValue };

            webChat.Patch(webChatPatch);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, webChat.WebChatStartDateandTime);
        }

        [Test]
        public void WebChatTests_CheckWebChatEndDateandTimeIsUpdated_WhenPatchIsCalled()
        {
            var webChat = new Models.WebChat { WebChatEndDateandTime = DateTime.UtcNow };
            var webChatPatch = new Models.WebChatPatch { WebChatEndDateandTime = DateTime.MaxValue };

            webChat.Patch(webChatPatch);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, webChat.WebChatEndDateandTime);
        }

        [Test]
        public void WebChatTests_CheckWebChatDurationIsUpdated_WhenPatchIsCalled()
        {
            var webChat = new Models.WebChat { WebChatDuration = TimeSpan.Zero };
            var webChatPatch = new Models.WebChatPatch { WebChatDuration = TimeSpan.MaxValue };

            webChat.Patch(webChatPatch);

            // Assert
            Assert.AreEqual(TimeSpan.MaxValue, webChat.WebChatDuration);
        }

        [Test]
        public void WebChatTests_CheckWebChatNarrativeIsUpdated_WhenPatchIsCalled()
        {
            var webChat = new Models.WebChat { WebChatNarrative = "Empty" };
            var webChatPatch = new Models.WebChatPatch { WebChatNarrative = "WebChatNarrative" };

            webChat.Patch(webChatPatch);

            // Assert
            Assert.AreEqual("WebChatNarrative", webChat.WebChatNarrative);
        }

        [Test]
        public void WebChatTests_CheckSentToCustomerIsUpdated_WhenPatchIsCalled()
        {
            var webChat = new Models.WebChat { SentToCustomer = true };
            var webChatPatch = new Models.WebChatPatch { SentToCustomer = false };

            webChat.Patch(webChatPatch);

            // Assert
            Assert.AreEqual(false, webChat.SentToCustomer);
        }
        
        [Test]
        public void WebChatTests_CheckDateandTimeSentToCustomersIsUpdated_WhenPatchIsCalled()
        {
            var webChat = new Models.WebChat { DateandTimeSentToCustomers = DateTime.UtcNow };
            var webChatPatch = new Models.WebChatPatch { DateandTimeSentToCustomers = DateTime.MaxValue };

            webChat.Patch(webChatPatch);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, webChat.DateandTimeSentToCustomers);
        }

        [Test]
        public void WebChatTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            var webChat = new Models.WebChat { LastModifiedDate = DateTime.UtcNow };
            var webChatPatch = new Models.WebChatPatch { LastModifiedDate = DateTime.MaxValue };

            webChat.Patch(webChatPatch);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, webChat.LastModifiedDate);
        }

        [Test]
        public void WebChatTests_CheckLastModifiedByUpdated_WhenPatchIsCalled()
        {
            var webChat = new Models.WebChat { LastModifiedTouchpointId = "0000000000" };
            var webChatPatch = new Models.WebChatPatch { LastModifiedTouchpointId = "0000000111" };

            webChat.Patch(webChatPatch);

            // Assert
            Assert.AreEqual("0000000111", webChat.LastModifiedTouchpointId);
        }
    }
}