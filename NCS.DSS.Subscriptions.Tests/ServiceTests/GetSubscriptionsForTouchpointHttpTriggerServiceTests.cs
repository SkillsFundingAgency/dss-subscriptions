using Moq;
using NCS.DSS.Subscriptions.Cosmos.Provider;
using NCS.DSS.Subscriptions.GetSubscriptionsForTouchpointHttpTrigger.Service;

namespace NCS.DSS.Subscriptions.Tests.ServiceTests
{
    [TestFixture]
    public class GetSubscriptionsForTouchpointHttpTriggerServiceTests
    {
        private readonly IGetSubscriptionsForTouchpointHttpTriggerService _getSubscriptionsForTouchpointHttpTriggerService;
        private readonly Mock<IDocumentDBProvider> _documentDbProvider;
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");
        private const string _touchPointId = "0000000001";
        public GetSubscriptionsForTouchpointHttpTriggerServiceTests()
        {
            _documentDbProvider = new Mock<IDocumentDBProvider>();
            _getSubscriptionsForTouchpointHttpTriggerService = new GetSubscriptionsForTouchpointHttpTriggerService(_documentDbProvider.Object);
        }
        [Test]
        public async Task GetSubscriptionsForTouchpointHttpTriggerServiceTests_GetSubscriptionsForTouchpointAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            // Arrange
            _documentDbProvider.Setup(x => x.GetSubscriptionsForTouchpointAsync(_customerId, _touchPointId)).Returns(Task.FromResult<List<Models.Subscriptions>>(null));

            // Act
            var result = await _getSubscriptionsForTouchpointHttpTriggerService.GetSubscriptionsForTouchpointAsync(_customerId, _touchPointId);

            // Assert
            Assert.That(result, Is.Null);
        }
        [Test]
        public async Task GetSubscriptionsForTouchpointHttpTriggerServiceTests_GetSubscriptionsForTouchpointAsync_ReturnsResource()
        {
            // Arrange
            _documentDbProvider.Setup(x => x.GetSubscriptionsForTouchpointAsync(_customerId, _touchPointId)).Returns(Task.FromResult(new List<Models.Subscriptions>()));

            // Act
            var result = await _getSubscriptionsForTouchpointHttpTriggerService.GetSubscriptionsForTouchpointAsync(_customerId, _touchPointId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<List<Models.Subscriptions>>());
        }
    }
}
