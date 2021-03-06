﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.WebChat.Cosmos.Helper;
using NCS.DSS.WebChat.GetWebChatByIdHttpTrigger.Service;
using NCS.DSS.WebChat.Helpers;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.WebChat.Tests
{
    [TestFixture]
    public class GetWebChatByIdHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidWebChatId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private ILogger _log;
        private HttpRequestMessage _request;
        private IResourceHelper _resourceHelper;
        private IGetWebChatByIdHttpTriggerService _getWebChatByIdHttpTriggerService;
        private IHttpRequestMessageHelper _httpRequestMessageHelper;
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
                            $"WebChats/d5369b9a-6959-4bd3-92fc-1583e72b7e51")
            };

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _getWebChatByIdHttpTriggerService = Substitute.For<IGetWebChatByIdHttpTriggerService>();
            _httpRequestMessageHelper = Substitute.For<IHttpRequestMessageHelper>();
            _httpRequestMessageHelper.GetTouchpointId(_request).Returns("0000000001");
        }

        [Test]
        public async Task GetWebChatByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestMessageHelper.GetTouchpointId(_request).Returns((string)null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetWebChatByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetWebChatByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetWebChatByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenWebChatIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetWebChatByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetWebChatByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetWebChatByIdHttpTrigger_ReturnsStatusCodeOk_WhenWebChatDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _getWebChatByIdHttpTriggerService.GetWebChatForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.WebChat>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetWebChatByIdHttpTrigger_ReturnsStatusCodeOk_WhenWebChatExists()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _getWebChatByIdHttpTriggerService.GetWebChatForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_webChat).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId, string actionPlanId)
        {
            return await GetWebChatByIdHttpTrigger.Function.GetWebChatByIdHttpTrigger.Run(
                _request, _log, customerId, interactionId, actionPlanId, _resourceHelper, _httpRequestMessageHelper, _getWebChatByIdHttpTriggerService).ConfigureAwait(false);
        }

    }
}