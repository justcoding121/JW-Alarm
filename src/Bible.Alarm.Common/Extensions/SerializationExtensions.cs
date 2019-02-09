using Newtonsoft.Json;

namespace JW.Alarm
{
    public static class SerializationExtensions
    {
        public static T As<T>(this string value) where T:new()
        {
           return JsonConvert.DeserializeObject<T>(value);
        }

        public static string AsSerialized<T>(this T value) where T : new()
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}
