using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Subscriptions.Cosmos.Helper;
using NCS.DSS.Subscriptions.Helpers;
using NCS.DSS.Subscriptions.Models;
using NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Service;
using NCS.DSS.Subscriptions.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using PatchSubscriptionsHttpTriggerRun = NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Function.PatchSubscriptionsHttpTrigger;

namespace NCS.DSS.Subscriptions.Tests.FunctionTests
{
    [TestFixture]
    public class PatchSubscriptionsHttpTriggerTests
    {
        // Constants
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private const string ValidSubscriptionId = "58b43e3f-4a50-4900-9c82-a14682ee90fa";
        private const string _touchPointId = "0000000001";
        private readonly string _apimUrl = "http://localhost:7001";

        // local variables
        private HttpRequest _request;
        private Mock<IValidate> _validate;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IConvertToDynamic> _convertToDynamic;
        private Mock<IPatchSubscriptionsHttpTriggerService> _patchSubscriptionsHttpTriggerService;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Models.Subscriptions _subscriptions;
        private SubscriptionsPatch _subscriptionsPatch;
        private PatchSubscriptionsHttpTriggerRun _patchSubscriptionsHttpTriggerRun;
        private List<ValidationResult> _validationResults;
        [SetUp]
        public void Setup()
        {
            _subscriptions = new Models.Subscriptions();
            _subscriptionsPatch = new Models.SubscriptionsPatch();
            _request = (new DefaultHttpContext()).Request;
            _validate = new Mock<IValidate>();
            _validationResults = new List<ValidationResult> { };
            _resourceHelper = new Mock<IResourceHelper>();
            _convertToDynamic = new Mock<IConvertToDynamic>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            var loggerHelper = new Mock<ILogger<PatchSubscriptionsHttpTriggerRun>>();
            _patchSubscriptionsHttpTriggerService = new Mock<IPatchSubscriptionsHttpTriggerService>();
            _patchSubscriptionsHttpTriggerRun = new PatchSubscriptionsHttpTriggerRun(
                _resourceHelper.Object,
                _httpRequestHelper.Object,
                _validate.Object,
                _patchSubscriptionsHttpTriggerService.Object,
                _convertToDynamic.Object,
                loggerHelper.Object
                );

        }

        [Test]
        public async Task PatchSubscriptionsHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(string.Empty);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidSubscriptionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task PatchSubscriptionsHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(_touchPointId);

            // Act
            var result = await RunFunction(InValidId, ValidSubscriptionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchSubscriptionsHttpTrigger_ReturnsStatusCodeBadRequest_WhenSubscriptionIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(_touchPointId);

            // Act
            var result = await RunFunction(ValidCustomerId, InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchSubscriptionsHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenSubscriptionHasInvalidRequest()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(_touchPointId);
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<SubscriptionsPatch>(_request)).Returns(Task.FromResult<SubscriptionsPatch>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidSubscriptionId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PatchSubscriptionsHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenSubscriptionHasFailedValidation()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(_touchPointId);
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);

            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<SubscriptionsPatch>(_request)).Returns(Task.FromResult(_subscriptionsPatch));

            _validationResults.Add(new ValidationResult("customer Id is Required"));
            _validate.Setup(x => x.ValidateResource(_subscriptionsPatch)).Returns(_validationResults);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidSubscriptionId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PatchSubscriptionsHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(_touchPointId);
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);

            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<SubscriptionsPatch>(_request)).Returns(Task.FromResult(_subscriptionsPatch));

            _validationResults.Clear();
            _validate.Setup(x => x.ValidateResource(It.IsAny<ISubscription>())).Returns(_validationResults);

            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidSubscriptionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }


        [Test]
        public async Task PatchSubscriptionsHttpTrigger_ReturnsStatusCodeNoContent_WhenSubscriptionDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(_touchPointId);
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<SubscriptionsPatch>(_request)).Returns(Task.FromResult(_subscriptionsPatch));

            _validationResults.Clear();
            _validate.Setup(x => x.ValidateResource(It.IsAny<ISubscription>())).Returns(_validationResults);

            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            _patchSubscriptionsHttpTriggerService.Setup(x => x.GetSubscriptionsForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Subscriptions>(null));
            // Act
            var result = await RunFunction(ValidCustomerId, ValidSubscriptionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }


        [Test]
        public async Task PatchSubscriptionsHttpTrigger_ReturnsStatusCodeOk_WhenRequestIsValid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(_touchPointId);
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<SubscriptionsPatch>(_request)).Returns(Task.FromResult(_subscriptionsPatch));

            _validationResults.Clear();
            _validate.Setup(x => x.ValidateResource(It.IsAny<ISubscription>())).Returns(_validationResults);

            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            _patchSubscriptionsHttpTriggerService.Setup(x => x.GetSubscriptionsForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(_subscriptions));
            _patchSubscriptionsHttpTriggerService.Setup(x => x.UpdateAsync(_subscriptions, _subscriptionsPatch)).Returns(Task.FromResult(_subscriptions));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidSubscriptionId);
            var jsonResult = (JsonResult)result;

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(jsonResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }
        private async Task<IActionResult> RunFunction(string customerId, string subscriptionId)
        {
            return await _patchSubscriptionsHttpTriggerRun.Run(
                _request,
                customerId, subscriptionId).ConfigureAwait(false);
        }
    }
}
