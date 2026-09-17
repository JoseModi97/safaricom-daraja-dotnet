using System;
using System.IO;
using System.Text.Json.Nodes;
using Safaricom.Daraja.Models;

namespace Safaricom.Daraja.Cli;

public static class CliConfigLoader
{
    public static DarajaConfig LoadConfig()
    {
        var config = new DarajaConfig();

        if (!string.IsNullOrWhiteSpace(config.ConsumerKey) && !string.IsNullOrWhiteSpace(config.ConsumerSecret))
        {
            return config;
        }

        var current = Directory.GetCurrentDirectory();
        while (!string.IsNullOrWhiteSpace(current))
        {
            var devJson = Path.Combine(current, "appsettings.Development.json");
            var prodJson = Path.Combine(current, "appsettings.json");

            if (TryLoadFromJson(devJson, config) || TryLoadFromJson(prodJson, config))
            {
                break;
            }

            var parent = Directory.GetParent(current);
            if (parent == null || parent.FullName == current) break;
            current = parent.FullName;
        }

        return config;
    }

    private static bool TryLoadFromJson(string filePath, DarajaConfig config)
    {
        if (!File.Exists(filePath)) return false;

        try
        {
            var text = File.ReadAllText(filePath);
            var root = JsonNode.Parse(text)?.AsObject();
            if (root == null) return false;

            var node = (root["Daraja"] as JsonObject) ?? root;

            var consumerKey = node["ConsumerKey"]?.ToString();
            var consumerSecret = node["ConsumerSecret"]?.ToString();
            var environment = node["Environment"]?.ToString();
            var baseAddress = node["BaseAddress"]?.ToString();

            var updated = false;
            if (string.IsNullOrWhiteSpace(config.ConsumerKey) && !string.IsNullOrWhiteSpace(consumerKey)) { config.ConsumerKey = consumerKey; updated = true; }
            if (string.IsNullOrWhiteSpace(config.ConsumerSecret) && !string.IsNullOrWhiteSpace(consumerSecret)) { config.ConsumerSecret = consumerSecret; updated = true; }
            if (!string.IsNullOrWhiteSpace(environment) && Enum.TryParse<DarajaEnvironment>(environment, true, out var parsedEnv)) { config.Environment = parsedEnv; }
            if (!string.IsNullOrWhiteSpace(baseAddress)) { config.BaseAddress = baseAddress; }

            return updated && !string.IsNullOrWhiteSpace(config.ConsumerKey);
        }
        catch
        {
            return false;
        }
    }
}
