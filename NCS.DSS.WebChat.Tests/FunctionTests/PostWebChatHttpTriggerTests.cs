using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.WebChat.Cosmos.Helper;
using NCS.DSS.WebChat.Helpers;
using NCS.DSS.WebChat.PostWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace NCS.DSS.WebChat.Tests
{
    [TestFixture]
    public class PostWebChatHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private ILogger _log;
        private HttpRequestMessage _request;
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private IHttpRequestMessageHelper _httpRequestMessageHelper;
        private IPostWebChatHttpTriggerService _postWebChatHttpTriggerService;
        private Models.WebChat _webChat;

        [SetUp]
        public void Setup()
        {
            _webChat = Substitute.For<Models.WebChat>();

            _request = new HttpRequestMessage()
            {
                Content = new StringContent(string.Empty),
                RequestUri =
                    new Uri($"http://localhost:7071/api/Customers/7E467BDB-213F-407A-B86A-1954053D3C24/" +
                            $"Customers/7E467BDB-213F-407A-B86A-1954053D3C24/" +
                            $"Interactions/1e1a555c-9633-4e12-ab28-09ed60d51cb3/" +
                            $"WebChats")
            };

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _httpRequestMessageHelper = Substitute.For<IHttpRequestMessageHelper>();
            _validate = Substitute.For<IValidate>();
            _postWebChatHttpTriggerService = Substitute.For<IPostWebChatHttpTriggerService>();
            _httpRequestMessageHelper.GetTouchpointId(_request).Returns("0000000001");
            _httpRequestMessageHelper.GetApimURL(_request).Returns("http://localhost:7071/");
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestMessageHelper.GetTouchpointId(_request).Returns((string)null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenWebChatHasFailedValidation()
        {
            _httpRequestMessageHelper.GetWebChatFromRequest<Models.WebChat>(_request).Returns(Task.FromResult(_webChat).Result);

            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.ValidateResource(Arg.Any<Models.WebChat>(), true).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenWebChatRequestIsInvalid()
        {
            _httpRequestMessageHelper.GetWebChatFromRequest<Models.WebChat>(_request).Throws(new JsonException());

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _httpRequestMessageHelper.GetWebChatFromRequest<Models.WebChat>(_request).Returns(Task.FromResult(_webChat).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _httpRequestMessageHelper.GetWebChatFromRequest<Models.WebChat>(_request).Returns(Task.FromResult(_webChat).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateWebChatRecord()
        {
            _httpRequestMessageHelper.GetWebChatFromRequest<Models.WebChat>(_request).Returns(Task.FromResult(_webChat).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _postWebChatHttpTriggerService.CreateAsync(Arg.Any<Models.WebChat>()).Returns(Task.FromResult<Models.WebChat>(null).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsNotValid()
        {
            _httpRequestMessageHelper.GetWebChatFromRequest<Models.WebChat>(_request).Returns(Task.FromResult(_webChat).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _postWebChatHttpTriggerService.CreateAsync(Arg.Any<Models.WebChat>()).Returns(Task.FromResult<Models.WebChat>(null).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            _httpRequestMessageHelper.GetWebChatFromRequest<Models.WebChat>(_request).Returns(Task.FromResult(_webChat).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _postWebChatHttpTriggerService.CreateAsync(Arg.Any<Models.WebChat>()).Returns(Task.FromResult(_webChat).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId)
        {
            return await PostWebChatHttpTrigger.Function.PostWebChatHttpTrigger.Run(
                _request, _log, customerId, interactionId, _resourceHelper, _httpRequestMessageHelper, _validate, _postWebChatHttpTriggerService).ConfigureAwait(false);
        }

    }
}