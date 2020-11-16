using System;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using PatchDto;

namespace PatchDtoTests
{
    [TestFixture]
    public class PatchTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void A_property_is_auto_patched_if_it_is_present_in_the_json_and_is_selected_with_an_expression()
        {
           
            const string json = @"
                {
                    'StringProperty' : 'json_value',
                    'IntProperty' : 5,
                    'DateTimeProperty' : '2020-01-12'
                }";

            var patch = new Patch<SourceClass>(json);

            var targetObject = new TargetClass
            {
                StringProperty = "original_value",
                IntProperty = 1,
                DateTimeProperty = DateTime.UtcNow
            };

            patch.AutoPatch(targetObject, 
                x => x.StringProperty,
                x => x.IntProperty,
                x => x.DateTimeProperty
                );

            Assert.AreEqual(targetObject.StringProperty, "json_value");
            Assert.AreEqual(targetObject.IntProperty, 5);
            Assert.AreEqual(targetObject.DateTimeProperty, new DateTime(2020, 1, 12));
        }


        [Test]
        public void A_property_is_excluded_from_auto_patch_if_is_not_selected_by_an_expression()
        {

            const string json = @"
                {
                    'StringProperty' : 'json_value',
                    'IntProperty' : 5,
                    'DateTimeProperty' : '2020-01-12'
                }";

            var patch = new Patch<SourceClass>(json);

            var targetObject = new TargetClass
            {
                StringProperty = "original_value",
                IntProperty = 1,
                DateTimeProperty = DateTime.UtcNow
            };

            patch.AutoPatch(targetObject,
                x => x.IntProperty,
                x => x.DateTimeProperty
            );

            Assert.AreEqual(targetObject.StringProperty, "original_value");
            Assert.AreEqual(targetObject.IntProperty, 5);
            Assert.AreEqual(targetObject.DateTimeProperty, new DateTime(2020, 1, 12));
        }

        [Test]
        public void A_property_is_excluded_from_auto_patch_if_is_not_present_in_the_json()
        {

            const string json = @"
                {
                    'IntProperty' : 5,
                    'DateTimeProperty' : '2020-01-12'
                }";

            var patch = new Patch<SourceClass>(json);

            var targetObject = new TargetClass
            {
                StringProperty = "original_value",
                IntProperty = 1,
                DateTimeProperty = DateTime.UtcNow
            };

            patch.AutoPatch(targetObject,
                x => x.StringProperty,
                x => x.IntProperty,
                x => x.DateTimeProperty
            );

            Assert.AreEqual(targetObject.StringProperty, "original_value");
            Assert.AreEqual(targetObject.IntProperty, 5);
            Assert.AreEqual(targetObject.DateTimeProperty, new DateTime(2020, 1, 12));
        }

        [Test]
        public void Patch_value_is_not_applied_if_validation_fails()
        {

            const string json = @"
                {
                    'IntProperty' : 15,
                    'StringProperty' : 'json_value'
                }";

            var patch = new Patch<SourceClass>(json);

            var targetObject = new TargetClass
            {
                StringProperty = "original_value",
                IntProperty = 1
            };

            patch.AutoPatch(targetObject,
                x => x.StringProperty,
                x => x.IntProperty
            );

            Assert.AreEqual(targetObject.StringProperty, "json_value");
            Assert.AreEqual(targetObject.IntProperty, 1);
        }

        [Test]
        public void Validation_failures_are_added_to_the()
        {

            const string json = @"
                {
                    'MaxLength5' : '123456'
                }";

            var patch = new Patch<SourceClass>(json);

            var targetObject = new TargetClass
            {
                MaxLength5 = "original",
            };

            patch.AutoPatch(targetObject,
                x => x.MaxLength5
            );

            var errors = patch.GetErrors();

            var propertyErrors = errors["MaxLength5"];

            Assert.AreEqual(propertyErrors.Count, 2);

            StringAssert.Contains("maximum length of '5'", propertyErrors[0]);
            StringAssert.Contains("[a-z]", propertyErrors[1]);
        }
    }


    public class SourceClass
    {
        public string StringProperty { get; set; }

        [System.ComponentModel.DataAnnotations.Range(1, 10)]
        public int IntProperty { get; set; }

        [MaxLength(5)]
        [RegularExpression("[a-z]")]
        public string MaxLength5 { get; set; }

        public DateTime DateTimeProperty { get; set; }

        public Guid GuidProperty { get; set; }
    }

    public class TargetClass
    {
        public string StringProperty { get; set; }
        public int IntProperty { get; set; }
        public DateTime DateTimeProperty { get; set; }
        public string MaxLength5 { get; set; }
        public Guid GuidProperty { get; set; }
    }
}