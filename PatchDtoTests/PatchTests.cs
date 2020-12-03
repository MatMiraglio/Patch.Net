using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using Patch.Net;

namespace Patch.NetTests
{
    [TestFixture]
    public class PatchTests
    {
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

            Assert.AreEqual("json_value", targetObject.StringProperty);
            Assert.AreEqual(5, targetObject.IntProperty);
            Assert.AreEqual(new DateTime(2020, 1, 12), targetObject.DateTimeProperty);
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

            Assert.AreEqual("original_value", targetObject.StringProperty);
            Assert.AreEqual(5, targetObject.IntProperty);
            Assert.AreEqual(new DateTime(2020, 1, 12), targetObject.DateTimeProperty);
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

            Assert.AreEqual("original_value", targetObject.StringProperty);
            Assert.AreEqual(5, targetObject.IntProperty);
            Assert.AreEqual(new DateTime(2020, 1, 12), targetObject.DateTimeProperty);
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

            Assert.AreEqual("json_value", targetObject.StringProperty);
            Assert.AreEqual(1, targetObject.IntProperty);
        }

        [Test]
        public void HasPatchFor_returns_true_if_the_key_was_present_in_the_json_and_outputs_the_value_to_use_in_patch()
        {

            const string json = @"
            {
                'intProperty' : 15,
                'stringProperty' : 'json_value'
            }";

            var patch = new Patch<SourceClass>(json);


            if (patch.HasKey(x => x.StringProperty, out var value))
                Assert.AreEqual("json_value", value);
            else
                Assert.Fail();

            if (patch.HasKey(x => x.IntProperty, out var intVal))
                Assert.AreEqual(15, intVal);
            else
                Assert.Fail();

        }

        [Test]
        public void HasPatchFor_returns_false_if_the_key_was_not_present_in_the_json()
        {
            var patch = new Patch<SourceClass>("{}");

            if (patch.HasKey(x => x.StringProperty, out var value))
                Assert.Fail();
        }

        [Test]
        public void Patch_is_case_insensitive()
        {

            const string json = @"
            {
                'stringproperty' : 'json_value',
                'INTPROPERTY' : 10
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

            Assert.AreEqual("json_value", targetObject.StringProperty);
            Assert.AreEqual(10, targetObject.IntProperty);
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

            var errors = patch.ValidationErrors;

            var propertyErrors = errors["MaxLength5"];

            Assert.True(patch.HasErrors);

            Assert.AreEqual(propertyErrors.Count, 2);

            StringAssert.Contains("maximum length of '5'", propertyErrors[0]);
            StringAssert.Contains("[a-z]", propertyErrors[1]);
        }

        [Test]
        public void Errors_are_returned_with_same_casing_as_in_the_original_json()
        {

            const string json = @"
            {
                'maxLength5' : '123456'
            }";

            var patch = new Patch<SourceClass>(json);

            var targetObject = new TargetClass
            {
                MaxLength5 = "original",
            };

            patch.AutoPatch(targetObject,
                x => x.MaxLength5
            );

            var errors = patch.ValidationErrors;

            var propertyErrors = errors["maxLength5"];

            Assert.True(patch.HasErrors);

            Assert.AreEqual(propertyErrors.Count, 2);

            StringAssert.Contains("maximum length of '5'", propertyErrors[0]);
            StringAssert.Contains("[a-z]", propertyErrors[1]);
        }

        [Test]
        public void AutoPatch_works_with_null()
        {

            const string json = @"
            {
                'stringProperty' : null
            }";

            var patch = new Patch<SourceClass>(json);

            var targetObject = new TargetClass
            {
                StringProperty = "original",
            };

            patch.AutoPatch(targetObject,
                x => x.StringProperty
            );

            Assert.IsNull(targetObject.StringProperty);
        }

        [Test]
        public void Patch_with_list()
        {

            const string json = @"
            {
                'guidList' : [
                    '00000000-0000-0000-0000-000000000022',
                    '00000000-0000-0000-0000-000000000033'
                ]
            }";

            var patch = new Patch<SourceClass>(json);

            if (patch.HasKey(x => x.GuidList, out var guidList))
            {
                Assert.AreEqual(new Guid("00000000-0000-0000-0000-000000000022"), guidList[0]);
                Assert.AreEqual(new Guid("00000000-0000-0000-0000-000000000033"), guidList[1]);
            }
            else Assert.Fail();
        }

        [Test]
        public void Patching_nested_properties_is_valid()
        {

            const string json = @"
            {
                'Nested' : {
                    'NestedProperty' : 'nested_value'
                }
            }";

            var patch = new Patch<SourceClass>(json);

            var targetObject = new TargetClass
            {
                Nested = new Nested
                {
                    NestedProperty = "original_nested"
                }
            };

            patch.AutoPatch(targetObject,
                x => x.Nested.NestedProperty
            );

            Assert.AreEqual("nested_value", targetObject.Nested.NestedProperty);
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

        public List<Guid> GuidList { get; set; }

        public Nested Nested { get; set; }
    }

    public class Nested
    {
        public string NestedProperty { get; set; }
    }

    public class TargetClass
    {
        public string StringProperty { get; set; }
        public int IntProperty { get; set; }
        public DateTime DateTimeProperty { get; set; }
        public string MaxLength5 { get; set; }
        public List<Guid> GuidProperty { get; set; }
        public Nested Nested { get; set; }
    }
}