using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.WebChat.Cosmos.Provider;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.WebChat.Tests
{
    [TestFixture]
    public class GetWebChatByIdHttpTriggerTests
    {
        //private readonly Guid _interactionId = Guid.Parse("aa57e39e-4469-4c79-a9e9-9cb4ef410382");
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidWebChatId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private HttpRequest _request;
        private Mock<ICosmosDBProvider> _cosmosDbProvider;
        private Mock<IHttpRequestHelper> _httpRequestMessageHelper;
        private Models.WebChat _webChat;
        private GetWebChatByIdHttpTrigger.Function.GetWebChatByIdHttpTrigger function;
        private Mock<ILogger<GetWebChatByIdHttpTrigger.Function.GetWebChatByIdHttpTrigger>> _log;

        [SetUp]
        public void Setup()
        {
            _webChat = new Models.WebChat();

            _request = new DefaultHttpContext().Request;
            _log = new Mock<ILogger<GetWebChatByIdHttpTrigger.Function.GetWebChatByIdHttpTrigger>>();
            _cosmosDbProvider = new Mock<ICosmosDBProvider>();
            _httpRequestMessageHelper = new Mock<IHttpRequestHelper>();

            function = new GetWebChatByIdHttpTrigger.Function.GetWebChatByIdHttpTrigger(_cosmosDbProvider.Object,
                _httpRequestMessageHelper.Object, _log.Object);
        }

        [Test]
        public async Task GetWebChatByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns<string>(null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetWebChatByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetWebChatByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidWebChatId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetWebChatByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenWebChatIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetWebChatByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            //Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetWebChatByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            //Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _cosmosDbProvider.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetWebChatByIdHttpTrigger_ReturnsStatusCodeOk_WhenWebChatDoesNotExist()
        {
            //Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _cosmosDbProvider.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));

            _cosmosDbProvider.Setup(x => x.GetWebChatForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.WebChat>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidWebChatId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetWebChatByIdHttpTrigger_ReturnsStatusCodeOk_WhenWebChatExists()
        {
            //Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _cosmosDbProvider.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));

            _cosmosDbProvider.Setup(x => x.GetWebChatForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.WebChat>(_webChat));
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidWebChatId);
            var responseResult = result as JsonResult;

            //Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId, string interactionId, string actionPlanId)
        {
            return await function.RunAsync(
                _request, customerId, interactionId, actionPlanId).ConfigureAwait(false);
        }
    }
}