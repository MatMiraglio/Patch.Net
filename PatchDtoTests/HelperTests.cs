using NUnit.Framework;
using Patch.Net;

namespace Patch.NetTests
{
    [TestFixture]
    public class HelperTests
    {

        [Test]
        public void GetPropertyName_returns_the_name_of_the_property_selected_in_an_expression()
        {
            var result = Helper.GetPropertyName<SampleClass>(x => x.StringProperty);

            Assert.AreEqual(result, "StringProperty");
        }

        internal class SampleClass
        {
            public string StringProperty { get; set; }
        }
    }
}
