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

        /// <summary>
        /// 序列化对象为 JSON 字符串
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="settings">JSON 序列化设置，为 null 时使用默认设置</param>
        /// <returns>序列化后的 JSON 字符串</returns>
        public static string Serialize(object obj, JsonSerializerSettings settings = null)
        {
            return JsonConvert.SerializeObject(obj, settings ?? DefaultSettings);
        }

        /// <summary>
        /// 反序列化 JSON 字符串为指定类型的对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="json">JSON 字符串</param>
        /// <param name="settings">JSON 序列化设置，为 null 时使用默认设置</param>
        /// <returns>反序列化后的对象</returns>
        public static T Deserialize<T>(string json, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeObject<T>(json, settings ?? DefaultSettings);
        }

        /// <summary>
        /// 反序列化 JSON 字符串为指定类型的对象
        /// </summary>
        /// <param name="json">JSON 字符串</param>
        /// <param name="type">目标类型</param>
        /// <param name="settings">JSON 序列化设置，为 null 时使用默认设置</param>
        /// <returns>反序列化后的对象</returns>
        public static object Deserialize(string json, Type type, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeObject(json, type, settings ?? DefaultSettings);
        }

        /// <summary>
        /// 尝试反序列化 JSON 字符串为指定类型的对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="json">JSON 字符串</param>
        /// <param name="result">反序列化后的对象</param>
        /// <param name="settings">JSON 序列化设置，为 null 时使用默认设置</param>
        /// <returns>反序列化是否成功</returns>
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
