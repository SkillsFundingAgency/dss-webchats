using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.WebChat.Cosmos.Provider;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.WebChat.Tests
{
    [TestFixture]
    public class GetWebChatHttpTriggerTest
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "56ad471f-bb4a-4551-8cde-32e2c38c043a";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private Mock<ILogger<GetWebChatHttpTrigger.Function.GetWebChatHttpTrigger>> _log;
        private HttpRequest _request;
        private Mock<ICosmosDBProvider> _cosmosDbProvider;
        private Mock<IHttpRequestHelper> _httpRequestMessageHelper;
        private GetWebChatHttpTrigger.Function.GetWebChatHttpTrigger function;

        [SetUp]
        public void Setup()
        {
            _request = new DefaultHttpContext().Request;
            _log = new Mock<ILogger<GetWebChatHttpTrigger.Function.GetWebChatHttpTrigger>>();
            _cosmosDbProvider = new Mock<ICosmosDBProvider>();
            _httpRequestMessageHelper = new Mock<IHttpRequestHelper>();

            function = new GetWebChatHttpTrigger.Function.GetWebChatHttpTrigger(_cosmosDbProvider.Object,
                _httpRequestMessageHelper.Object, _log.Object);
        }

        [Test]
        public async Task GetWebChatHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns<string>(null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetWebChatHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetWebChatHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetWebChatHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            //Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetWebChatHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            //Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _cosmosDbProvider.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetWebChatHttpTrigger_ReturnsStatusCodeNoContent_WhenWebChatDoesNotExist()
        {
            //Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _cosmosDbProvider.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));

            _cosmosDbProvider.Setup(x => x.GetWebChatsForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<List<Models.WebChat>>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetWebChatHttpTrigger_ReturnsStatusCodeOk_WhenWebChatExists()
        {
            //Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _cosmosDbProvider.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true)  );
            var listOfWebChates = new List<Models.WebChat>();
            _cosmosDbProvider.Setup(x => x.GetWebChatsForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<List<Models.WebChat>>(listOfWebChates));
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId); var responseResult = result as JsonResult;

            //Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId, string interactionId)
        {
            return await function.RunAsync(
                _request, customerId, interactionId).ConfigureAwait(false);
        }

    }
}