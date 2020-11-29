using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Patch.Net
{


    public class PatchJsonConverter<T> : JsonConverter<Patch<T>>
    {
        public override Patch<T> ReadJson(JsonReader reader, Type objectType, [AllowNull] Patch<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return new Patch<T>(reader.ToString());
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] Patch<T> value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }



    public class Patch<TSource>
    {
        private readonly JObject _json;
        private readonly TSource _object;
        private readonly Dictionary<string, List<string>> _errors;

        public Patch(string json)
        {
            _json = JObject.Parse(json);
            _object = JsonConvert.DeserializeObject<TSource>(json);
            _errors = new Dictionary<string, List<string>>();
        }

        public Dictionary<string, List<string>> GetErrors()
        {
            return _errors;
        }

        public bool HasErrors => _errors.Count > 0;

        /// <summary>
        /// Used to patch the properties in the target object when the name in the Dto matches and all the validation can be done via attributes.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="propertiesToPatch">properties to auto patch</param>
        public void AutoPatch(object target, params Expression<Func<TSource, object>>[] propertiesToPatch)
        {
            foreach (var propertySelector in propertiesToPatch)
            {
                var propertyName = Helper.GetPropertyName(propertySelector);

                if (!HasPatchFor(propertyName)) continue;


                var value = Helper.GetValue<TSource>(propertyName, from: _object);

                var validationResults = new List<ValidationResult>();
                var vc = new ValidationContext(_object){MemberName = propertyName};

                bool isValid = Validator.TryValidateProperty(value, vc, validationResults);

                if (isValid)
                {
                    PatchValue(target, propertyName);
                }
                else
                {
                    var originalJsonName = GetOriginalJsonName(propertyName);


                    foreach (var validationResult in validationResults)
                    {


                        if (_errors.TryGetValue(originalJsonName, out var propertyErrors))
                        {
                            propertyErrors.Add(validationResult.ErrorMessage);
                        }
                        else
                        {
                            _errors.Add(originalJsonName, new List<string> { validationResult.ErrorMessage });
                        }
                    }
                }
            }
        }

        private string GetOriginalJsonName(string propertyName)
        {
            foreach (var property in _json.Properties())
            {
                if (property.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return property.Name;
                }
            }

            throw new InvalidOperationException($"Key {propertyName} is not present in Json");
        }

        private void PatchValue(object target, string propertyName)
        {
            var value = Helper.GetValue<TSource>(propertyName, @from: _object);

            Helper.Assign(value, propertyName, to: target);
        }

        public bool HasPatchFor(string propertyName)
        {
            return _json.GetValue(propertyName, StringComparison.InvariantCultureIgnoreCase) != null;
        }

        public bool HasPatchFor(Expression<Func<TSource, object>> expression, out object value)
        {
            var propertyName = Helper.GetPropertyName(expression);
            var jsonValue = _json[propertyName];

            if (jsonValue == null)
            {
                value = default;
                return false;
            }

            value = Helper.GetValue<TSource>(propertyName, @from: _object);

            return true;
        }
    }
}
