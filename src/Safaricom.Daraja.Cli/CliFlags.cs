using System;
using System.Collections.Generic;

namespace Safaricom.Daraja.Cli;

public static class CliFlags
{
    public static Dictionary<string, string> Parse(string[] args)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg.StartsWith("--"))
            {
                var key = arg.Substring(2);
                if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                {
                    dict[key] = args[i + 1];
                    i++;
                }
                else
                {
                    dict[key] = "true";
                }
            }
            else if (arg.StartsWith("-"))
            {
                var key = arg.Substring(1);
                dict[key] = "true";
            }
        }
        return dict;
    }

    public static void Fail(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
        Environment.Exit(1);
    }

    public static string Require(Dictionary<string, string> flags, string key, string usageHint)
    {
        var value = flags.GetValueOrDefault(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            Fail($"Missing required --{key}\n\n{usageHint}");
        }
        return value!;
    }
}
