using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using NCS.DSS.Subscriptions.Cosmos.Provider;
using NCS.DSS.Subscriptions.Models;
using NCS.DSS.Subscriptions.PatchSubscriptionsHttpTrigger.Service;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;

namespace NCS.DSS.Subscriptions.Tests.ServiceTests
{
    [TestFixture]
    public class PatchSubscriptionsHttpTriggerServiceTests
    {
        private readonly IPatchSubscriptionsHttpTriggerService _patchSubscriptionsHttpTriggerService;
        private readonly Mock<IDocumentDBProvider> _documentDbProvider;
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");
        private readonly Guid _subscriptionId = Guid.Parse("4C2DD229-DF9D-4899-AC09-A21DC13EB176");
        private readonly Models.Subscriptions _subscriptions;
        private SubscriptionsPatch _subscriptionsPatch;
        public PatchSubscriptionsHttpTriggerServiceTests()
        {
            _subscriptions = new Models.Subscriptions();
            _subscriptionsPatch = new SubscriptionsPatch();
            _documentDbProvider = new Mock<IDocumentDBProvider>();
            _patchSubscriptionsHttpTriggerService = new PatchSubscriptionsHttpTriggerService(_documentDbProvider.Object);
        }
        [Test]
        public async Task PatchSubscriptionsHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenResourceCannotBeFound()
        {            
            // Act
            var result = await _patchSubscriptionsHttpTriggerService.UpdateAsync(null,_subscriptionsPatch);

            // Assert
            Assert.That(result, Is.Null);
        }
        [Test]
        public async Task PatchSubscriptionsHttpTriggerServiceTests_UpdateAsync_ReturnsResourceUpdated()
        {
            // Arrange
            const string documentServiceResponseClass = "Microsoft.Azure.Documents.DocumentServiceResponse, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            const string dictionaryNameValueCollectionClass = "Microsoft.Azure.Documents.Collections.DictionaryNameValueCollection, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

            var resourceResponse = new ResourceResponse<Document>(new Document());
            var documentServiceResponseType = Type.GetType(documentServiceResponseClass);

            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var headers = new NameValueCollection { { "x-ms-request-charge", "0" } };

            var headersDictionaryType = Type.GetType(dictionaryNameValueCollectionClass);

            var headersDictionaryInstance = Activator.CreateInstance(headersDictionaryType, headers);

            var arguments = new[] { Stream.Null, headersDictionaryInstance, HttpStatusCode.OK, null };

            var documentServiceResponse = documentServiceResponseType?.GetTypeInfo().GetConstructors(flags)[0].Invoke(arguments);

            var responseField = typeof(ResourceResponse<Document>).GetTypeInfo().GetField("response", flags);

            responseField?.SetValue(resourceResponse, documentServiceResponse);
           
            _documentDbProvider.Setup(x => x.UpdateSubscriptionsAsync(_subscriptions)).Returns(Task.FromResult(resourceResponse));

            // Act
            var result = await _patchSubscriptionsHttpTriggerService.UpdateAsync(_subscriptions,_subscriptionsPatch);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Subscriptions>());
        }

        [Test]
        public async Task PatchSubscriptionsHttpTriggerServiceTests_GetSubscriptionsForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            // Arrange
            _documentDbProvider.Setup(x => x.GetSubscriptionsForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(_subscriptions));

            // Act
            var result = await _patchSubscriptionsHttpTriggerService.GetSubscriptionsForCustomerAsync(_customerId, _subscriptionId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Subscriptions>());
        }
    }
}
