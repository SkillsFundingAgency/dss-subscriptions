using DFC.Common.Standard.GuidHelper;
using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Subscriptions.Cosmos.Helper;
using NCS.DSS.Subscriptions.GetSubscriptionsForTouchpointHttpTrigger.Service;
using System.Net;
using GetSubscriptionsForTouchpointHttpTriggerrRun = NCS.DSS.Subscriptions.GetSubscriptionsForTouchpointHttpTrigger.Function.GetSubscriptionsForTouchpointHttpTrigger;

namespace NCS.DSS.Subscriptions.Tests.FunctionTests
{
    [TestFixture]
    public class GetSubscriptionsForTouchpointHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private readonly Guid _customerId = Guid.Parse("1dd4d206-131a-44fd-8e2d-18b88b383f72");
        private const string _touchPointId = "0000000001";
        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IGetSubscriptionsForTouchpointHttpTriggerService> _getSubscriptionsForTouchpointHttpTriggerService;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private List<Models.Subscriptions> _subscriptions;
        private IGuidHelper _guidHelper;
        private GetSubscriptionsForTouchpointHttpTriggerrRun _getSubscriptionsForTouchpointHttpTrigger;
        [SetUp]
        public void Setup()
        {
            _subscriptions = new List<Models.Subscriptions>();
            _request = (new DefaultHttpContext()).Request;
            _resourceHelper = new Mock<IResourceHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            var loggerHelper = new Mock<ILogger<GetSubscriptionsForTouchpointHttpTriggerrRun>>();
            _guidHelper = new GuidHelper();
            _getSubscriptionsForTouchpointHttpTriggerService = new Mock<IGetSubscriptionsForTouchpointHttpTriggerService>();
            _getSubscriptionsForTouchpointHttpTrigger = new GetSubscriptionsForTouchpointHttpTriggerrRun(
                _resourceHelper.Object,
                _httpRequestHelper.Object,
                _getSubscriptionsForTouchpointHttpTriggerService.Object,
                loggerHelper.Object
                );

        }

        [Test]
        public async Task GetSubscriptionsForTouchpointHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(string.Empty);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task GetSubscriptionsForTouchpointHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(_touchPointId);

            // Act
            var result = await RunFunction(InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }
        [Test]
        public async Task GetSubscriptionsForTouchpointHttpTrigger_ReturnsStatusCodeNoContent_WhenSubscriptionDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(_touchPointId);
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _getSubscriptionsForTouchpointHttpTriggerService.Setup(x => x.GetSubscriptionsForTouchpointAsync(_customerId, _touchPointId)).Returns(Task.FromResult<List<Models.Subscriptions>>(null));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }
        [Test]
        public async Task GetSubscriptionsForTouchpointHttpTrigger_ReturnsStatusCodeOk_WhenSubscriptionExists()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(_touchPointId);
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _getSubscriptionsForTouchpointHttpTriggerService.Setup(x => x.GetSubscriptionsForTouchpointAsync(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(_subscriptions));

            // Act
            var result = await RunFunction(ValidCustomerId);
            var jsonResult = (JsonResult)result;

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(jsonResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }
        private async Task<IActionResult> RunFunction(string customerId)
        {
            return await _getSubscriptionsForTouchpointHttpTrigger.Run(
                _request,
                customerId).ConfigureAwait(false);
        }
    }
}
