using Microsoft.Azure.Cosmos;
using Moq;
using NCS.DSS.Subscriptions.Cosmos.Provider;
using NCS.DSS.Subscriptions.Models;
using NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Service;
using System.Net;

namespace NCS.DSS.Subscriptions.Tests.ServiceTests
{
    [TestFixture]
    public class PatchSubscriptionsHttpTriggerServiceTests
    {
        private readonly IPatchSubscriptionsHttpTriggerService _patchSubscriptionsHttpTriggerService;
        private readonly Mock<ICosmosDBProvider> _cosmosDbProvider;
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");
        private readonly Guid _subscriptionId = Guid.Parse("4C2DD229-DF9D-4899-AC09-A21DC13EB176");
        private readonly Models.Subscriptions _subscriptions;
        private SubscriptionsPatch _subscriptionsPatch;
        public PatchSubscriptionsHttpTriggerServiceTests()
        {
            _subscriptions = new Models.Subscriptions();
            _subscriptionsPatch = new SubscriptionsPatch();
            _cosmosDbProvider = new Mock<ICosmosDBProvider>();
            _patchSubscriptionsHttpTriggerService = new PatchSubscriptionsHttpTriggerService(_cosmosDbProvider.Object);
        }
        [Test]
        public async Task PatchSubscriptionsHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            // Act
            var result = await _patchSubscriptionsHttpTriggerService.UpdateAsync(null, _subscriptionsPatch);

            // Assert
            Assert.That(result, Is.Null);
        }
        [Test]
        public async Task PatchSubscriptionsHttpTriggerServiceTests_UpdateAsync_ReturnsResourceUpdated()
        {
            // Arrange
            var resourceResponse = new Mock<ItemResponse<Models.Subscriptions>>();
            resourceResponse.Setup(x => x.Resource).Returns(_subscriptions);
            resourceResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
            _cosmosDbProvider.Setup(x => x.UpdateSubscriptionsAsync(_subscriptions)).Returns(Task.FromResult(resourceResponse.Object));

            // Act
            var result = await _patchSubscriptionsHttpTriggerService.UpdateAsync(_subscriptions, _subscriptionsPatch);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Subscriptions>());
        }

        [Test]
        public async Task PatchSubscriptionsHttpTriggerServiceTests_GetSubscriptionsForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            // Arrange
            _cosmosDbProvider.Setup(x => x.GetSubscriptionsForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(_subscriptions));

            // Act
            var result = await _patchSubscriptionsHttpTriggerService.GetSubscriptionsForCustomerAsync(_customerId, _subscriptionId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Subscriptions>());
        }
    }
}
