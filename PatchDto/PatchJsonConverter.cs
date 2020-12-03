using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

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
}
