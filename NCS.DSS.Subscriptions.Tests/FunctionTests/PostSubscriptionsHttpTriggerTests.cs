using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Subscriptions.Cosmos.Helper;
using NCS.DSS.Subscriptions.Helpers;
using NCS.DSS.Subscriptions.Models;
using NCS.DSS.Subscriptions.PostSubscriptionsHttpTrigger.Service;
using NCS.DSS.Subscriptions.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using PostSubscriptionsHttpTriggerRun = NCS.DSS.Subscriptions.PostSubscriptionsHttpTrigger.Function.PostSubscriptionsHttpTrigger;

namespace NCS.DSS.Subscriptions.Tests.FunctionTests
{
    [TestFixture]
    public class PostSubscriptionsHttpTriggerTests
    {
        // Constants
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private readonly Guid _customerId = Guid.Parse("1dd4d206-131a-44fd-8e2d-18b88b383f72");
        private readonly Guid? _subscriptionId = Guid.Parse("0f0ce9cd-5f0e-41f7-84e7-6532e01691ae");
        private const string _touchPointId = "0000000001";
        private readonly string _apimUrl = "http://localhost:7001";

        // local variables
        private HttpRequest _request;
        private Mock<IValidate> _validate;
        private List<ValidationResult> _validationResults;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IConvertToDynamic> _convertToDynamic;
        private Mock<IPostSubscriptionsHttpTriggerService> _postSubscriptionsHttpTriggerService;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Models.Subscriptions _subscriptions;
        private PostSubscriptionsHttpTriggerRun _postSubscriptionsHttpTriggerRun;
        [SetUp]
        public void Setup()
        {
            _subscriptions = new Models.Subscriptions();
            _request = (new DefaultHttpContext()).Request;
            _validate = new Mock<IValidate>();
            _validationResults = new List<ValidationResult> { };
            _resourceHelper = new Mock<IResourceHelper>();
            _convertToDynamic = new Mock<IConvertToDynamic>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            var loggerHelper = new Mock<ILogger<PostSubscriptionsHttpTriggerRun>>();
            _postSubscriptionsHttpTriggerService = new Mock<IPostSubscriptionsHttpTriggerService>();
            _postSubscriptionsHttpTriggerRun = new PostSubscriptionsHttpTriggerRun(
                _resourceHelper.Object,
                _httpRequestHelper.Object,
                _validate.Object,
                _postSubscriptionsHttpTriggerService.Object,
                loggerHelper.Object,
                _convertToDynamic.Object
                );

        }

        [Test]
        public async Task PostSubscriptionsHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(string.Empty);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task PostSubscriptionsHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(_touchPointId);

            // Act
            var result = await RunFunction(InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostSubscriptionsHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenSubscriptionHasInvalidRequest()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(_touchPointId);
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Subscriptions>(_request)).Returns(Task.FromResult<Models.Subscriptions>(null));

            _postSubscriptionsHttpTriggerService.Setup(x => x.CreateAsync(_subscriptions)).Returns(Task.FromResult(_subscriptions));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PostSubscriptionsHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenSubscriptionHasFailedValidation()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(_touchPointId);
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Subscriptions>(_request)).Returns(Task.FromResult(_subscriptions));

            _validationResults.Add(new ValidationResult("customer Id is Required"));
            _validate.Setup(x => x.ValidateResource(_subscriptions)).Returns(_validationResults);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PostSubscriptionsHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(_touchPointId);
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Subscriptions>(_request)).Returns(Task.FromResult(_subscriptions));

            _validationResults.Clear();
            _validate.Setup(x => x.ValidateResource(It.IsAny<ISubscription>())).Returns(_validationResults);

            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }
        [Test]
        public async Task PostSubscriptionsHttpTrigger_ReturnsStatusCodeConflict_WhenSubscriptionHasDuplicate()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(_touchPointId);
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Subscriptions>(_request)).Returns(Task.FromResult(_subscriptions));

            _validationResults.Clear();
            _validate.Setup(x => x.ValidateResource(It.IsAny<ISubscription>())).Returns(_validationResults);

            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesSubscriptionExist(It.IsAny<Guid>(), It.IsAny<string>())).Returns((Task.FromResult(_subscriptionId)));
            _validate.Setup(x => x.ValidateResultForDuplicateSubscriptionId(It.IsAny<Guid>())).Returns(await Task.FromResult(new SubscriptionError() { }));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<ConflictResult>());
        }

        [Test]
        public async Task PostSubscriptionsHttpTrigger_ReturnsStatusCodeBadRequest_WhenSubscriptionDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(_touchPointId);
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Subscriptions>(_request)).Returns(Task.FromResult(_subscriptions));

            _validationResults.Clear();
            _validate.Setup(x => x.ValidateResource(It.IsAny<ISubscription>())).Returns(_validationResults);

            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesSubscriptionExist(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult<Guid?>(null));

            _postSubscriptionsHttpTriggerService.Setup(x => x.CreateAsync(_subscriptions)).Returns(Task.FromResult<Models.Subscriptions>(null));
            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }


        [Test]
        public async Task PostSubscriptionsHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns(_touchPointId);
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Subscriptions>(_request)).Returns(Task.FromResult(_subscriptions));

            _validationResults.Clear();
            _validate.Setup(x => x.ValidateResource(It.IsAny<ISubscription>())).Returns(_validationResults);

            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesSubscriptionExist(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult<Guid?>(null));

            _postSubscriptionsHttpTriggerService.Setup(x => x.CreateAsync(_subscriptions)).Returns(Task.FromResult(_subscriptions));

            // Act
            var result = await RunFunction(ValidCustomerId);
            var jsonResult = (JsonResult)result;

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(jsonResult.StatusCode, Is.EqualTo((int)HttpStatusCode.Created));
        }
        private async Task<IActionResult> RunFunction(string customerId)
        {
            return await _postSubscriptionsHttpTriggerRun.Run(
                _request,
                customerId).ConfigureAwait(false);
        }
    }
}
