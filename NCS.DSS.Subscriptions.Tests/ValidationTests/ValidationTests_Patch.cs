using NCS.DSS.Subscriptions.Validation;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Subscriptions.Tests.ValidationTests
{
    [TestFixture]
    public class ValidationTests_Patch
    {
        private IValidate _validate;

        [SetUp]
        public void Setup()
        {
            _validate = new Validate();
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedByIsInvalid()
        {
            var transfer = new Models.SubscriptionsPatch
            {
                LastModifiedBy = "[0000000001]"
            };

            var result = _validate.ValidateResource(transfer);

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedByIsValid()
        {
            var transfer = new Models.SubscriptionsPatch
            {
                LastModifiedBy = "0000000001"
            };

            var result = _validate.ValidateResource(transfer);

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

    }
}
