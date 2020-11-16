using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PatchDto
{
    public class Patch<TSource>
    {
        private readonly JObject _json;
        private readonly TSource _object;
        private readonly Dictionary<string, List<string>> _errors;
        private ValidationContext _validationContext;

        public Patch(string json)
        {
            _json = JObject.Parse(json);
            _object = JsonConvert.DeserializeObject<TSource>(json);
            _errors = new Dictionary<string, List<string>>();
            _validationContext = new ValidationContext(_object);

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
                    foreach (var validationResult in validationResults)
                    {
                        if (_errors.TryGetValue(propertyName, out var propertyErrors))
                        {
                            propertyErrors.Add(validationResult.ErrorMessage);
                        }
                        else
                        {
                            _errors.Add(propertyName, new List<string> { validationResult.ErrorMessage });
                        }
                    }
                }
            }
        }

        private void PatchValue(object target, string propertyName)
        {
            var value = Helper.GetValue<TSource>(propertyName, from: _object);

            Helper.Assign(value, propertyName, to: target);
        }

        public bool HasPatchFor(string propertyName)
        {
            return _json[propertyName] != null;
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

            value = Helper.GetValue<TSource>(propertyName, from: _object);

            return true;
        }
    }
}
