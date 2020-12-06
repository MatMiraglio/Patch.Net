using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;

namespace Patch.Net
{
    public class PatchJsonConverter<T> : JsonConverter<Patch<T>>
    {
        public override Patch<T> ReadJson(JsonReader reader, Type objectType, [AllowNull] Patch<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            using (var sw = new StringWriter(CultureInfo.InvariantCulture))
            using (var jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.WriteToken(reader);

                return new Patch<T>(sw.ToString());
            }
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] Patch<T> value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
