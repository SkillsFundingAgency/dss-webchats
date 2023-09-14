using NCS.DSS.WebChat.Validation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCS.DSS.WebChat.Models;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.WebChat.Tests.ValidateTests
{
    [TestFixture]
    public class ValidateTests
    {
        private IValidate _validate;

        [SetUp]
        public void Setup()
        {
            _validate = new Validate();
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenDigitalReferenceIsInvalid()
        {
            var webChat = new Models.WebChat
            {
                DigitalReference = "Test[]abc+?",
                WebChatStartDateandTime = DateTime.UtcNow.AddMinutes(-10),
                WebChatEndDateandTime = DateTime.UtcNow.AddMinutes(-5),
                WebChatNarrative = "Some narrative data here"
            };

            var result = _validate.ValidateResource(webChat, true);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenDigitalReferenceIsValid()
        {
            var webChat = new Models.WebChat
            {
                DigitalReference = "TestValue123",
                WebChatStartDateandTime = DateTime.UtcNow.AddMinutes(-10),
                WebChatEndDateandTime = DateTime.UtcNow.AddMinutes(-5),
                WebChatNarrative = "Some narrative data here"
            };

            var result = _validate.ValidateResource(webChat, true);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenWebChatNarrativeIsInvalid()
        {
            var webChat = new Models.WebChat
            {
                DigitalReference = "TestValue123",
                WebChatStartDateandTime = DateTime.UtcNow.AddMinutes(-10),
                WebChatEndDateandTime = DateTime.UtcNow.AddMinutes(-5),
                WebChatNarrative = string.Empty
            };

            var result = _validate.ValidateResource(webChat, true);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenWebChatNarrativeIsValid()
        {
            var webChat = new Models.WebChat
            {
                DigitalReference = "TestValue123",
                WebChatStartDateandTime = DateTime.UtcNow.AddMinutes(-10),
                WebChatEndDateandTime = DateTime.UtcNow.AddMinutes(-5),
                WebChatNarrative = "Some narrative data here"
            };

            var result = _validate.ValidateResource(webChat, true);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedTouchpointIdIsInvalid()
        {
            var webChat = new Models.WebChat
            {
                DigitalReference = "TestValue123",
                WebChatStartDateandTime = DateTime.UtcNow.AddMinutes(-10),
                WebChatEndDateandTime = DateTime.UtcNow.AddMinutes(-5),
                WebChatNarrative = "Some narrative data here",
                LastModifiedTouchpointId = "000000000A"
            };

            var result = _validate.ValidateResource(webChat, true);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedTouchpointIdIsValid()
        {
            var webChat = new Models.WebChat
            {
                DigitalReference = "TestValue123",
                WebChatStartDateandTime = DateTime.UtcNow.AddMinutes(-10),
                WebChatEndDateandTime = DateTime.UtcNow.AddMinutes(-5),
                WebChatNarrative = "Some narrative data here",
                LastModifiedTouchpointId = "0000000001"
            };

            var result = _validate.ValidateResource(webChat, true);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
    }
}
