using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Patch.Net
{

    public class PatchJsonConverter<T> : JsonConverter<Patch<T>>
    {
        public override Patch<T> ReadJson(JsonReader reader, Type objectType, [AllowNull] Patch<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] Patch<T> value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }


    [JsonConverter(typeof(PatchJsonConverter<>))]
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

            ValidatePatch();
        }

        private void ValidatePatch()
        {
            foreach (var key in _json.Properties())
            {
                var classPropertyName = GetClassPropertyName(key.Name);

                var value = Reflection.GetValue(classPropertyName, from: _object);

                var validationResults = GetValidationErrors(classPropertyName, value);

                AddErrors(key.Name, validationResults);
            }
        }

        private List<ValidationResult> GetValidationErrors(string classPropertyName, object value)
        {
            var validationResults = new List<ValidationResult>();

            Validator.TryValidateProperty(value, new ValidationContext(_object) { MemberName = classPropertyName }, validationResults);

            return validationResults;
        }

        private void AddErrors(string jsonKey, List<ValidationResult> validationResults)
        {
            foreach (var validationResult in validationResults)
            {
                if (_errors.TryGetValue(jsonKey, out var propertyErrors))
                {
                    propertyErrors.Add(validationResult.ErrorMessage);
                }
                else
                {
                    _errors.Add(jsonKey, new List<string> { validationResult.ErrorMessage });
                }
            }
        }

        private string GetClassPropertyName(string propertyName)
        {
            return typeof(TSource)
                .GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                .Name;
        }

        public IReadOnlyDictionary<string, List<string>> ValidationErrors => _errors;
        
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
                var propertyName = Reflection.GetPropertyName(propertySelector);

                if (KeyIsPresentInJson(propertyName) && JsonValueIsValid(propertyName))
                {
                    PatchValue(target, propertyName);
                }
            }
        }

        private bool JsonValueIsValid(string propertyName)
        {
            return !_errors.ContainsKey(propertyName);
        }

        private void PatchValue(object target, string propertyName)
        {
            var value = Reflection.GetValue(propertyName, from: _object);

            Reflection.Assign(value, propertyName, to: target);
        }

        private bool KeyIsPresentInJson(string propertyName)
        {
            return _json.GetValue(propertyName, StringComparison.InvariantCultureIgnoreCase) != null;
        }

        public bool HasKey<T>(Expression<Func<TSource, object>> expression, out T value)
        {
            var propertyName = Reflection.GetPropertyName(expression);

            if (KeyIsPresentInJson(propertyName))
            {
                value = (T) Reflection.GetValue(propertyName, from: _object);
                return true;
            }

            value = default;
            return false;
        }
    }
}
