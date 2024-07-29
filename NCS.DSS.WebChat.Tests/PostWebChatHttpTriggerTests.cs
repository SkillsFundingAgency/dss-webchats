using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.WebChat.Cosmos.Helper;
using NCS.DSS.WebChat.Models;
using NCS.DSS.WebChat.PostWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.Validation;
using Newtonsoft.Json;
using NUnit.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NCS.DSS.WebChat.Tests
{
    [TestFixture]
    public class PostWebChatHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private Models.WebChat _webChat;

        private Mock<ILogger<PostWebChatHttpTrigger.Function.PostWebChatHttpTrigger>> _log;
        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IHttpRequestHelper> _httpRequestMessageHelper;
        private Mock<IPostWebChatHttpTriggerService> _postWebChatHttpTriggerService;
        private PostWebChatHttpTrigger.Function.PostWebChatHttpTrigger function;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private Mock<IValidate> _validate;

        [SetUp]
        public void Setup()
        {
            _webChat = new Models.WebChat();
            _validate = new Mock<IValidate>();
            _request = new DefaultHttpContext().Request;

            _log = new Mock<ILogger<PostWebChatHttpTrigger.Function.PostWebChatHttpTrigger>>();
            _resourceHelper = new Mock<IResourceHelper>();
            _postWebChatHttpTriggerService = new Mock<IPostWebChatHttpTriggerService>();
            _httpRequestMessageHelper = new Mock<IHttpRequestHelper>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _jsonHelper = new JsonHelper();

            function = new PostWebChatHttpTrigger.Function.PostWebChatHttpTrigger(_resourceHelper.Object,
                _httpRequestMessageHelper.Object, _httpResponseMessageHelper, _jsonHelper, _validate.Object, _postWebChatHttpTriggerService.Object, _log.Object);
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns<string>(null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenWebChatHasFailedValidation()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.WebChat>(_request)).Returns(Task.FromResult(_webChat));

            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.Setup(x => x.ValidateResource(It.IsAny<Models.WebChat>(), true)).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenWebChatRequestIsInvalid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.WebChat>(_request)).Throws(new JsonException());
            //
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            //Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.WebChat>(_request)).Returns(Task.FromResult(_webChat));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.WebChat>(_request)).Returns(Task.FromResult(_webChat));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);


            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateWebChatRecord()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.WebChat>(_request)).Returns(Task.FromResult(_webChat));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _postWebChatHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.WebChat>())).Returns(Task.FromResult<Models.WebChat>(null));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsNotValid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.WebChat>(_request)).Returns(Task.FromResult(_webChat));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _postWebChatHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.WebChat>())).Returns(Task.FromResult<Models.WebChat>(null));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostWebChatHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.WebChat>(_request)).Returns(Task.FromResult(_webChat));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _postWebChatHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.WebChat>())).Returns(Task.FromResult<Models.WebChat>(_webChat));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId);
            var responseResult = result as JsonResult;

            //Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.Created));
        }

        private async Task<IActionResult> RunFunction(string customerId, string interactionId)
        {
            return await function.Run(
                _request, customerId, interactionId).ConfigureAwait(false);
        }

    }
}