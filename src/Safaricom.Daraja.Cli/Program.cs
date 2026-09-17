using System;
using System.Reflection;
using System.Threading.Tasks;
using Safaricom.Daraja.Cli.Commands;

namespace Safaricom.Daraja.Cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var command = args.Length > 0 ? args[0] : "init";
        var restArgs = args.Length > 1 ? args[1..] : Array.Empty<string>();

        if (command is "--help" or "-h" or "help")
        {
            PrintHelp();
            return 0;
        }

        if (command is "--version" or "-v" or "version")
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.1.0";
            Console.WriteLine($"safaricom-daraja v{version}");
            return 0;
        }

        PrintBanner();

        try
        {
            switch (command.ToLowerInvariant())
            {
                case "init":
                    await InitCommand.ExecuteAsync(restArgs);
                    return 0;

                case "token":
                    await TokenCommand.ExecuteAsync(restArgs);
                    return 0;

                case "stk":
                    await StkCommand.ExecuteAsync(restArgs);
                    return 0;

                case "test":
                    await TestCommand.ExecuteAsync(restArgs);
                    return 0;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Unknown command: {command}\n");
                    Console.ResetColor();
                    PrintHelp();
                    return 1;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nError: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }

    public static void PrintBanner()
    {
        Console.WriteLine("\x1b[32m=========================================================\x1b[0m");
        Console.WriteLine("\x1b[1;32m      Safaricom Daraja SDK Setup Wizard (CLI)            \x1b[0m");
        Console.WriteLine("\x1b[32m=========================================================\x1b[0m");
        Console.WriteLine("Interactive configurator for ASP.NET Core Minimal APIs & MVC\n");
    }

    public static void PrintHelp()
    {
        Console.WriteLine(@"
Usage:
  safaricom-daraja [command] [options]
  dotnet safaricom-daraja [command] [options]

Commands:
  init            Interactive setup wizard to configure credentials & scaffold sample code (Default)
  token           Fetch and print an OAuth access token - quick sanity check of your credentials
  stk             Raise an STK Push prompt, or query the outcome of a previous one
  test            Run offline cryptography self-checks (SecurityCredential RSA round-trip, STK password format)
  help, --help    Show this help message
  --version, -v   Show version

Options for 'init':
  --consumer-key <key>       Daraja app Consumer Key
  --consumer-secret <secret> Daraja app Consumer Secret
  --environment <env>        Sandbox | Production (default: Sandbox)
  --shortcode <code>         Default Paybill/Till shortcode used in scaffolded samples
  --passkey <key>            Lipa na M-Pesa Online passkey used in scaffolded samples
  --framework <name>         Target: minimal | mvc | console
  --yes, -y                  Skip prompts and use defaults or provided flags

Options for 'token':
  (credentials read from appsettings / DARAJA_* env vars)

Options for 'stk':
  --shortcode <code>      Paybill/Till shortcode (required)
  --passkey <key>         Lipa na M-Pesa Online passkey (required)
  --phone <msisdn>        Payer's phone number (required to push; e.g. 254712345678)
  --amount <n>            Amount to charge (default: 1)
  --callback-url <url>    Your STK callback URL (required to push)
  --account-reference <r> Account reference (default: auto-generated)
  --description <text>    Transaction description (default: ""Payment"")
  --query <id>            Query the outcome of an existing CheckoutRequestID instead of pushing
");
    }
}
