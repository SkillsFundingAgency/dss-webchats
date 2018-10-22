using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.WebChat.Validation;
using NUnit.Framework;

namespace NCS.DSS.WebChat.Tests.ValidationTests
{
    [TestFixture]
    public class ValidateTests
    {
        
        [Test]
        public void ValidateTests_ReturnValidationResult_WhenWebChatIsNotSuppliedForPost()
        {
            var webChat = new Models.WebChat();

            var validation = new Validate();

            var result = validation.ValidateResource(webChat, false);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenWebChatStartDateandTimeIsNotSuppliedForPost()
        {
            var webChat = new Models.WebChat
            {
                WebChatEndDateandTime = DateTime.UtcNow,
                WebChatNarrative = "WebChatNarrative"
            };

            var validation = new Validate();

            var result = validation.ValidateResource(webChat,true);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenWebChatEndDateandTimeIsNotSuppliedForPost()
        {
            var webChat = new Models.WebChat
            {
                WebChatStartDateandTime = DateTime.UtcNow,
                WebChatNarrative = "WebChatNarrative"
            };

            var validation = new Validate();

            var result = validation.ValidateResource(webChat, true);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }


        [Test]
        public void ValidateTests_ReturnValidationResult_WhenWebChatNarrativeIsNotSuppliedForPost()
        {
            var webChat = new Models.WebChat
            {
                WebChatStartDateandTime = DateTime.UtcNow,
                WebChatEndDateandTime = DateTime.UtcNow,
            };

            var validation = new Validate();

            var result = validation.ValidateResource(webChat, true);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }
        
        [Test]
        public void ValidateTests_ReturnValidationResult_WhenWebChatStartDateandTimeIsInTheFuture()
        {
            var webChat = new Models.WebChat
            {
                WebChatStartDateandTime = DateTime.MaxValue,
                WebChatEndDateandTime = DateTime.UtcNow,
                WebChatNarrative = "WebChatNarrative"
            };

            var validation = new Validate();

            var result = validation.ValidateResource(webChat, false);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenWebChatEndDateandTimeIsInTheFuture()
        {
            var webChat = new Models.WebChat
            {
                WebChatStartDateandTime = DateTime.UtcNow,
                WebChatEndDateandTime = DateTime.MaxValue,
                WebChatNarrative = "WebChatNarrative"
            };

            var validation = new Validate();

            var result = validation.ValidateResource(webChat, false);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenDateandTimeSentToCustomersIsInTheFuture()
        {
            var webChat = new Models.WebChat
            {
                WebChatStartDateandTime = DateTime.UtcNow,
                WebChatEndDateandTime = DateTime.UtcNow,
                WebChatNarrative = "WebChatNarrative",
                DateandTimeSentToCustomers = DateTime.MaxValue
            };

            var validation = new Validate();

            var result = validation.ValidateResource(webChat, false);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedDateIsInTheFuture()
        {
            var webChat = new Models.WebChat
            {
                WebChatStartDateandTime = DateTime.UtcNow,
                WebChatEndDateandTime = DateTime.UtcNow,
                WebChatNarrative = "WebChatNarrative",
                LastModifiedDate = DateTime.MaxValue
            };

            var validation = new Validate();

            var result = validation.ValidateResource(webChat, false);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }
    }
}