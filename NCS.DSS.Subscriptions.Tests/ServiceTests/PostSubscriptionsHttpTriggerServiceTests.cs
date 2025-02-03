using Microsoft.Azure.Cosmos;
using Moq;
using NCS.DSS.Subscriptions.Cosmos.Provider;
using NCS.DSS.Subscriptions.PostSubscriptionsHttpTrigger.Service;
using System.Net;

namespace NCS.DSS.Subscriptions.Tests.ServiceTests
{
    [TestFixture]
    public class PostSubscriptionsHttpTriggerServiceTests
    {
        private readonly IPostSubscriptionsHttpTriggerService _postSubscriptionsHttpTriggerService;
        private readonly Mock<ICosmosDBProvider> _cosmosDbProvider;

        private readonly Models.Subscriptions _subscriptions;
        public PostSubscriptionsHttpTriggerServiceTests()
        {
            _subscriptions = new Models.Subscriptions();
            _cosmosDbProvider = new Mock<ICosmosDBProvider>();
            _postSubscriptionsHttpTriggerService = new PostSubscriptionsHttpTriggerService(_cosmosDbProvider.Object);
        }
        [Test]
        public async Task PostSubscriptionsHttpTriggerServiceTests_CreateAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            // Act
            var result = await _postSubscriptionsHttpTriggerService.CreateAsync(null);

            // Assert
            Assert.That(result, Is.Null);
        }
        [Test]
        public async Task PostSubscriptionsHttpTriggerServiceTests_CreateAsync_ReturnsResource()
        {
            // Arrange
            var resourceResponse = new Mock<ItemResponse<Models.Subscriptions>>();
            resourceResponse.Setup(x => x.Resource).Returns(_subscriptions);
            resourceResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.Created);
            _cosmosDbProvider.Setup(x => x.CreateSubscriptionsAsync(_subscriptions)).Returns(Task.FromResult(resourceResponse.Object));

            // Act
            var result = await _postSubscriptionsHttpTriggerService.CreateAsync(_subscriptions);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Subscriptions>());
        }
    }
}
