using System.IO;
using Cant.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cant.Provider;
internal static class ConfigProvider
{
    public static Config ReadConfig => JsonConvert.DeserializeObject<Config>(File.ReadAllText("appsettings.json"), new StringEnumConverter())!;
}
