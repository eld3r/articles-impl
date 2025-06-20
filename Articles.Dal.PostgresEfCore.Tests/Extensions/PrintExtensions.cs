using Newtonsoft.Json;

namespace System;

public static class PrintExtensions
{
    public static T PrintToConsole<T>(this T value, string name = "")
    {
        Console.WriteLine($"{name} [{typeof(T).Name}]: {JsonConvert.SerializeObject(value, Formatting.Indented)}");
        return value;
    }
}