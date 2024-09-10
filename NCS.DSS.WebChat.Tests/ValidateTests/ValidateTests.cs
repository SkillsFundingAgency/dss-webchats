using NCS.DSS.WebChat.Validation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
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

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenWebChatNarrativeIsInvalid()
        {
            var webChat = new Models.WebChat
            {
                DigitalReference = "TestValue123",
                WebChatStartDateandTime = DateTime.UtcNow.AddMinutes(-10),
                WebChatEndDateandTime = DateTime.UtcNow.AddMinutes(-5),
                WebChatNarrative = "Test<>"
            };

            var result = _validate.ValidateResource(webChat, true);

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
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

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
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

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
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

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }
    }
}
