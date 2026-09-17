using System;
using System.Threading.Tasks;
using Safaricom.Daraja.Auth;

namespace Safaricom.Daraja.Cli.Commands;

public static class TokenCommand
{
    public static async Task ExecuteAsync(string[] args)
    {
        var config = CliConfigLoader.LoadConfig();

        Console.WriteLine($"\x1b[36mFetching OAuth token from {config.Environment} ({config.GetEffectiveBaseAddress()})...\x1b[0m\n");

        try
        {
            var auth = new DarajaAuthClient(config);
            var token = await auth.GetValidAccessTokenAsync();

            var masked = token.Length > 8 ? $"{token[..4]}...{token[^4..]}" : "****";
            Console.WriteLine($"  \x1b[32m✔\x1b[0m Access token acquired: \x1b[1m{masked}\x1b[0m");
            Console.WriteLine("\nYour ConsumerKey/ConsumerSecret are valid for this environment.\n");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nFailed to fetch a token: {ex.Message}");
            Console.ResetColor();
            Environment.Exit(1);
        }
    }
}
