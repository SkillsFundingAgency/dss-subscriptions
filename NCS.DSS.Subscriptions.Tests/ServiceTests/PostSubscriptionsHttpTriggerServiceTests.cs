using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using NCS.DSS.Subscriptions.Cosmos.Provider;
using NCS.DSS.Subscriptions.PostSubscriptionsHttpTrigger.Service;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;

namespace NCS.DSS.Subscriptions.Tests.ServiceTests
{
    [TestFixture]
    public class PostSubscriptionsHttpTriggerServiceTests
    {
        private readonly IPostSubscriptionsHttpTriggerService _postSubscriptionsHttpTriggerService;
        private readonly Mock<IDocumentDBProvider> _documentDbProvider;

        private readonly Models.Subscriptions _subscriptions;  
        public PostSubscriptionsHttpTriggerServiceTests()
        {
            _subscriptions = new Models.Subscriptions();
            _documentDbProvider = new Mock<IDocumentDBProvider>();
            _postSubscriptionsHttpTriggerService = new PostSubscriptionsHttpTriggerService(_documentDbProvider.Object);
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
            const string documentServiceResponseClass = "Microsoft.Azure.Documents.DocumentServiceResponse, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            const string dictionaryNameValueCollectionClass = "Microsoft.Azure.Documents.Collections.DictionaryNameValueCollection, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

            var resourceResponse = new ResourceResponse<Document>(new Document());
            var documentServiceResponseType = Type.GetType(documentServiceResponseClass);

            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var headers = new NameValueCollection { { "x-ms-request-charge", "0" } };

            var headersDictionaryType = Type.GetType(dictionaryNameValueCollectionClass);

            var headersDictionaryInstance = Activator.CreateInstance(headersDictionaryType, headers);

            var arguments = new[] { Stream.Null, headersDictionaryInstance, HttpStatusCode.Created, null };

            var documentServiceResponse = documentServiceResponseType?.GetTypeInfo().GetConstructors(flags)[0].Invoke(arguments);

            var responseField = typeof(ResourceResponse<Document>).GetTypeInfo().GetField("response", flags);

            responseField?.SetValue(resourceResponse, documentServiceResponse);
           
            _documentDbProvider.Setup(x => x.CreateSubscriptionsAsync(_subscriptions)).Returns(Task.FromResult(resourceResponse));

            // Act
            var result = await _postSubscriptionsHttpTriggerService.CreateAsync(_subscriptions);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Subscriptions>());
        }
    }
}
