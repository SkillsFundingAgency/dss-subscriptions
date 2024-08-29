
namespace NCS.DSS.Subscriptions.Tests.ModelTests
{
    [TestFixture]
    public class SubscriptionModelTests
    {
        private Models.Subscriptions _subscriptions;
        private readonly Guid _customerId = Guid.Parse("1dd4d206-131a-44fd-8e2d-18b88b383f72");
        private readonly Guid? _subscriptionId = Guid.Parse("0f0ce9cd-5f0e-41f7-84e7-6532e01691ae");
        private const string _touchPointId = "0000000001";
        [SetUp]
        public void Setup()
        {
            _subscriptions = new Models.Subscriptions();
        }
        [Test]
        public void SubscriptionModelTests_PopulatesDefaultValues_WhenSetDefaultValuesIsCalled()
        {

            // Act
            _subscriptions.SetDefaultValues();

            // Assert
            Assert.That(_subscriptions.LastModifiedDate, Is.Not.Null);

        }

        [Test]
        public void SubscriptionModelTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            // Arrange
            _subscriptions.LastModifiedDate = DateTime.MaxValue;

            // Act
            _subscriptions.SetDefaultValues();

            // Assert
            Assert.That(DateTime.MaxValue, Is.EqualTo(_subscriptions.LastModifiedDate));
        }

        [Test]
        public void SubscriptionModelTests_CheckActionIdIsSet_WhenSetIdsIsCalled()
        {
            // Act
            _subscriptions.SetIds(_customerId, _touchPointId);

            // Assert
            Assert.That(_subscriptions.SubscriptionId, Is.Not.Null);
        }

        [Test]
        public void SubscriptionModelTests_CheckCustomerIdIsSet_WhenSetIdsIsCalled()
        {

            // Act
            _subscriptions.SetIds(_customerId, _touchPointId);

            // Assert
            Assert.That(_customerId, Is.EqualTo(_subscriptions.CustomerId));
        }

        [Test]
        public void SubscriptionModelTests_CheckTouchpointIdIsSet_WhenSetIdsIsCalled()
        {

            // Act
            _subscriptions.SetIds(_customerId, _touchPointId);

            // Assert
            Assert.That(_touchPointId, Is.EqualTo(_subscriptions.TouchPointId));
        }
    }
}
