using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace OuderraadWielewaal.Extensions
{
    public static class SessionExtensions
    {
        // Sla een object op als JSON
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Lees JSON uit en zet het terug om naar een object
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}