using Newtonsoft.Json;

namespace Articles.Tests.Extensions;

public static class PrintExtensions
{
    public static T PrintToConsole<T>(this T value, string name = "")
    {
        Console.WriteLine(
            $"{name} [{typeof(T).Name}]: {JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore 
            })}");
        return value;
    }
}