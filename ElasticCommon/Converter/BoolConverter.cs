using System;
using Newtonsoft.Json;

namespace ElasticCommon.Converter
{
    public class BoolConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (string.IsNullOrWhiteSpace(Convert.ToString(value)))
            {
                writer.WriteValue(false);

                return;
            }

            writer.WriteValue(((bool)value) ? 1 : 0);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (string.IsNullOrWhiteSpace(Convert.ToString(reader.Value)))
            {
                return false;
            }

            return reader.Value.ToString() == "1";
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool);
        }
    }
}