using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.WebChat.Cosmos.Helper;
using NCS.DSS.WebChat.GetWebChatByIdHttpTrigger.Service;
using Moq;
using NUnit.Framework;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        //private HttpRequestMessage _request;
        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IHttpRequestHelper> _httpRequestMessageHelper;
        private Mock<IGetWebChatByIdHttpTriggerService> _getWebChatByIdHttpTriggerService;
        private Models.WebChat _webChat;
        private GetWebChatByIdHttpTrigger.Function.GetWebChatByIdHttpTrigger function;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private Mock<ILogger<GetWebChatByIdHttpTrigger.Function.GetWebChatByIdHttpTrigger>> _log;

        [SetUp]
        public void Setup()
        {
            _webChat = new Models.WebChat();

            _request = new DefaultHttpContext().Request;
            _log = new Mock<ILogger<GetWebChatByIdHttpTrigger.Function.GetWebChatByIdHttpTrigger>>();
            _resourceHelper = new Mock<IResourceHelper>();
            _getWebChatByIdHttpTriggerService = new Mock<IGetWebChatByIdHttpTriggerService>();
            _httpRequestMessageHelper = new Mock<IHttpRequestHelper>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _jsonHelper = new JsonHelper();

            function = new GetWebChatByIdHttpTrigger.Function.GetWebChatByIdHttpTrigger(_resourceHelper.Object,
                _httpRequestMessageHelper.Object, _httpResponseMessageHelper, _jsonHelper, _getWebChatByIdHttpTriggerService.Object, _log.Object);
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
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

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
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);

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
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);

            _getWebChatByIdHttpTriggerService.Setup(x=> x.GetWebChatForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.WebChat>(null));

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
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionResourceExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);

            _getWebChatByIdHttpTriggerService.Setup(x => x.GetWebChatForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.WebChat>(_webChat));
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidWebChatId);
            var responseResult = result as JsonResult;

            //Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId, string interactionId, string actionPlanId)
        {
            return await function.Run(
                _request, customerId, interactionId, actionPlanId).ConfigureAwait(false);
        }
    }
}