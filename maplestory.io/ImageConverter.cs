using Newtonsoft.Json;
using SixLabors.ImageSharp;
using System;
using System.IO;

namespace maplestory.io.Data
{
    public class ImageConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Image<Rgba32>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var m = new MemoryStream(Convert.FromBase64String((string)reader.Value));
            return Image.Load(m);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            lock (value)
            {
                Image<Rgba32> bmp = (Image<Rgba32>)value;
                MemoryStream m = new MemoryStream();
                bmp.SaveAsPng(m);

                writer.WriteValue(Convert.ToBase64String(m.ToArray()));
            }
        }
    }
}