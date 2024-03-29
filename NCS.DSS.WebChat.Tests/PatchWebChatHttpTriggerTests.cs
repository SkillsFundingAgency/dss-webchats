﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.WebChat.Cosmos.Helper;
using NCS.DSS.WebChat.Models;
using NCS.DSS.WebChat.PatchWebChatHttpTrigger.Service;
using NCS.DSS.WebChat.Validation;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NCS.DSS.WebChat.Tests
{
    [TestFixture]
    public class PatchWebChatHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidWebChatId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private Models.WebChat _webChat;
        private Models.WebChatPatch _webChatPatch;
        private Mock<ILogger> _log;
        private DefaultHttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IHttpRequestHelper> _httpRequestMessageHelper;
        private Mock<IPatchWebChatHttpTriggerService> _patchWebChatHttpTriggerService;
        private PatchWebChatHttpTrigger.Function.PatchWebChatHttpTrigger function;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private IValidate _validate;

        [SetUp]
        public void Setup()
        {
            _webChat = new Models.WebChat();
            _webChatPatch = new WebChatPatch();
            _validate = new Validate();
            _request = null;
            _log = new Mock<ILogger>();
            _resourceHelper = new Mock<IResourceHelper>();
            _patchWebChatHttpTriggerService = new Mock<IPatchWebChatHttpTriggerService>();
            _httpRequestMessageHelper = new Mock<IHttpRequestHelper>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _jsonHelper = new JsonHelper();

            function = new PatchWebChatHttpTrigger.Function.PatchWebChatHttpTrigger(_resourceHelper.Object,
                _httpRequestMessageHelper.Object, _httpResponseMessageHelper, _jsonHelper, _validate, _patchWebChatHttpTriggerService.Object);
        }

        [Test]
        public async Task PatchWebChatHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns<string>(null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchWebChatHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchWebChatHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchWebChatHttpTrigger_ReturnsStatusCodeBadRequest_WhenWebChatIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchWebChatHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenWebChatHasFailedValidation()
        {
            // Arrange
            var validate = new Mock<IValidate>();
            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            validate.Setup(x => x.ValidateResource(It.IsAny<Models.WebChatPatch>(),false)).Returns(validationResults);
            function = new PatchWebChatHttpTrigger.Function.PatchWebChatHttpTrigger(_resourceHelper.Object, _httpRequestMessageHelper.Object, _httpResponseMessageHelper, _jsonHelper, validate.Object, _patchWebChatHttpTriggerService.Object);
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.WebChatPatch>(_request)).Returns(Task.FromResult(_webChatPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchWebChatHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenWebChatRequestIsInvalid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.WebChatPatch>(_request)).Throws(new JsonException());

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchWebChatHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            //Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.WebChatPatch>(_request)).Returns(Task.FromResult(_webChatPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            //Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchWebChatHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.WebChatPatch>(_request)).Returns(Task.FromResult(_webChatPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);
            _patchWebChatHttpTriggerService.Setup(x => x.GetWebChatForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.WebChat>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchWebChatHttpTrigger_ReturnsStatusCodeNoContent_WhenWebChatDoesNotExist()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.WebChatPatch>(_request)).Returns(Task.FromResult(_webChatPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _patchWebChatHttpTriggerService.Setup(x => x.GetWebChatForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.WebChat>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }
        
        [Test]
        public async Task PatchWebChatHttpTrigger_ReturnsStatusCodeOk_WhenWebChatDoesNotExist()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.WebChatPatch>(_request)).Returns(Task.FromResult(_webChatPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _patchWebChatHttpTriggerService.Setup(x => x.GetWebChatForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.WebChat>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchWebChatHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToUpdateWebChatRecord()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.WebChatPatch>(_request)).Returns(Task.FromResult(_webChatPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _patchWebChatHttpTriggerService.Setup(x => x.GetWebChatForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.WebChat>(_webChat));
            _patchWebChatHttpTriggerService.Setup(x => x.UpdateAsync(It.IsAny<Models.WebChat>(), It.IsAny<Models.WebChatPatch>())).Returns(Task.FromResult<Models.WebChat>(null));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchWebChatHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsNotValid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.WebChatPatch>(_request)).Returns(Task.FromResult(_webChatPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _patchWebChatHttpTriggerService.Setup(x => x.GetWebChatForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.WebChat>(_webChat));
            _patchWebChatHttpTriggerService.Setup(x => x.UpdateAsync(It.IsAny<Models.WebChat>(), It.IsAny<Models.WebChatPatch>())).Returns(Task.FromResult<Models.WebChat>(null));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchWebChatHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsValid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.WebChatPatch>(_request)).Returns(Task.FromResult(_webChatPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _patchWebChatHttpTriggerService.Setup(x => x.GetWebChatForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.WebChat>(_webChat));
            _patchWebChatHttpTriggerService.Setup(x => x.UpdateAsync(It.IsAny<Models.WebChat>(), It.IsAny<Models.WebChatPatch>())).Returns(Task.FromResult<Models.WebChat>(_webChat));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId, string actionPlanId)
        {
            return await function.Run(
                _request, _log.Object, customerId, interactionId, actionPlanId).ConfigureAwait(false);
        }

    }
}