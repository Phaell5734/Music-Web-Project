using System.Text.Json;

namespace Uyumsoft_Projem.Helpers
{
    public static class SessionExt
    {
        static readonly JsonSerializerOptions _opt = new(JsonSerializerDefaults.Web);

        public static void SetObject<T>(this ISession ses, string key, T value) =>
            ses.SetString(key, JsonSerializer.Serialize(value, _opt));

        public static T? GetObject<T>(this ISession ses, string key) =>
            ses.TryGetValue(key, out _) ? JsonSerializer.Deserialize<T>(ses.GetString(key)!, _opt)
                                        : default;
    }

}
