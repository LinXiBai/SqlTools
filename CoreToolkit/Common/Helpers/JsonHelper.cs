using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CoreToolkit.Common.Helpers
{
    /// <summary>
    /// JSON 序列化/反序列化辅助工具类（基于 Newtonsoft.Json）
    /// </summary>
    public static class JsonHelper
    {
        private static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new JsonConverter[] { new StringEnumConverter() },
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            DateFormatString = "yyyy-MM-dd HH:mm:ss",
            Formatting = Formatting.Indented
        };

        public static string Serialize(object obj, JsonSerializerSettings settings = null)
        {
            return JsonConvert.SerializeObject(obj, settings ?? DefaultSettings);
        }

        public static T Deserialize<T>(string json, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeObject<T>(json, settings ?? DefaultSettings);
        }

        public static object Deserialize(string json, Type type, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeObject(json, type, settings ?? DefaultSettings);
        }

        public static bool TryDeserialize<T>(string json, out T result, JsonSerializerSettings settings = null)
        {
            try
            {
                result = JsonConvert.DeserializeObject<T>(json, settings ?? DefaultSettings);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
    }
}
