using NCS.DSS.Subscriptions.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCS.DSS.Subscriptions.Tests.ValidationTests
{
    [TestFixture]
    public class ValidationTests_Post
    {
        private IValidate _validate;

        [SetUp]
        public void Setup()
        {
            _validate = new Validate();
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenTouchPointIdIsInvalid()
        {
            var transfer = new Models.Subscriptions
            {
                TouchPointId = "000000000A"
            };

            var result = _validate.ValidateResource(transfer);

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenTouchPointIdIsValid()
        {
            var transfer = new Models.Subscriptions
            {
                TouchPointId = "0000000001"
            };

            var result = _validate.ValidateResource(transfer);

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedByIsInvalid()
        {
            var transfer = new Models.Subscriptions
            {
                TouchPointId = "0000000001",
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
            var transfer = new Models.Subscriptions
            {
                TouchPointId = "0000000001",
                LastModifiedBy = "0000000001"
            };

            var result = _validate.ValidateResource(transfer);

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }
    }
}
