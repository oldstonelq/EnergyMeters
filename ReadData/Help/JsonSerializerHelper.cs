using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

public static class JsonSerializerHelper
{
    // 序列化选项
    private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,           // 格式化 JSON 输出
        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(), // 驼峰命名
        NullValueHandling = NullValueHandling.Ignore, // 忽略 null 值
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore, // 处理循环引用
        DateFormatHandling = DateFormatHandling.IsoDateFormat, // ISO 日期格式
        Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() } // 枚举转字符串
    };

    /// <summary>
    /// 将对象序列化为 JSON 并保存到文件
    /// </summary>
    public static bool SaveToJsonFile<T>(T data, string filePath)
    {
        try
        {
            string jsonString = JsonConvert.SerializeObject(data, _settings);
            File.WriteAllText(filePath, jsonString);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"序列化出错: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 从 JSON 文件读取并反序列化为对象
    /// </summary>
    public static T LoadFromJsonFile<T>(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"文件不存在: {filePath}");
                return default;
            }

            string jsonString = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(jsonString, _settings);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"反序列化出错: {ex.Message}");
            return default;
        }
    }
    /// <summary>
    /// 将对象序列化为 JSON 字符串
    /// </summary>
    public static string ToJsonString<T>(T data)
    {
        return JsonConvert.SerializeObject(data, _settings);
    }

    /// <summary>
    /// 从 JSON 字符串反序列化为对象
    /// </summary>
    public static T FromJsonString<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, _settings);
    }
}    