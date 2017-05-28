using System;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;

namespace WZData
{
    public class ImageConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Bitmap);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var m = new MemoryStream(Convert.FromBase64String((string)reader.Value));
            return (Bitmap)Bitmap.FromStream(m);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            lock (value)
            {
                Bitmap bmp = (Bitmap)value;
                MemoryStream m = new MemoryStream();
                bmp.Save(m, System.Drawing.Imaging.ImageFormat.Png);

                writer.WriteValue(Convert.ToBase64String(m.ToArray()));
            }
        }
    }
}