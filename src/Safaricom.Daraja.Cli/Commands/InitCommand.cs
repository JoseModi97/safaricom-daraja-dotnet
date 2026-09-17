using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Safaricom.Daraja.Cli.Commands;

public static class InitCommand
{
    public static async Task ExecuteAsync(string[] args)
    {
        var targetDir = Directory.GetCurrentDirectory();
        var (detectedFramework, _) = ProjectScaffolder.DetectEnvironment(targetDir);

        var flags = ParseFlags(args);
        var autoYes = flags.ContainsKey("yes") || flags.ContainsKey("y");

        var consumerKey = flags.GetValueOrDefault("consumer-key") ?? Environment.GetEnvironmentVariable("DARAJA_CONSUMER_KEY") ?? string.Empty;
        var consumerSecret = flags.GetValueOrDefault("consumer-secret") ?? Environment.GetEnvironmentVariable("DARAJA_CONSUMER_SECRET") ?? string.Empty;
        var environment = flags.GetValueOrDefault("environment") ?? Environment.GetEnvironmentVariable("DARAJA_ENVIRONMENT") ?? "Sandbox";
        var shortCode = flags.GetValueOrDefault("shortcode") ?? "174379";
        var passkey = flags.GetValueOrDefault("passkey") ?? string.Empty;
        var framework = flags.GetValueOrDefault("framework") ?? detectedFramework;

        if (!autoYes)
        {
            Console.WriteLine("\n\x1b[1mStep 1: Enter your Daraja App Credentials\x1b[0m\n");

            consumerKey = Prompter.Ask("Daraja Consumer Key", consumerKey, s => !string.IsNullOrWhiteSpace(s), "Consumer Key cannot be empty.");
            consumerSecret = Prompter.AskSecret("Daraja Consumer Secret", consumerSecret, s => !string.IsNullOrWhiteSpace(s), "Consumer Secret cannot be empty.");

            var envChoices = new List<(string Label, string Value)>
            {
                ("Sandbox (developer testing)", "Sandbox"),
                ("Production (live money)", "Production")
            };
            environment = Prompter.Select("Target environment:", envChoices, environment.Equals("Production", StringComparison.OrdinalIgnoreCase) ? 1 : 0);

            Console.WriteLine("\n\x1b[1mStep 2: Sample Code Defaults\x1b[0m\n");

            shortCode = Prompter.Ask("Default Paybill/Till shortcode for scaffolded samples", shortCode);
            passkey = Prompter.AskSecret("Lipa na M-Pesa Online passkey for scaffolded samples", passkey);

            Console.WriteLine("\n\x1b[1mStep 3: Target Architecture\x1b[0m\n");

            var frameworkChoices = new List<(string Label, string Value)>
            {
                ("ASP.NET Core Minimal APIs (Endpoints/DarajaEndpoints.cs)", "minimal"),
                ("ASP.NET Core MVC (Controllers/DarajaController.cs)", "mvc"),
                ("Standalone Console script (DarajaDemo.cs)", "console")
            };

            var defaultIndex = framework == "mvc" ? 1 : (framework == "console" ? 2 : 0);
            framework = Prompter.Select("Select application style:", frameworkChoices, defaultIndex);

            var proceed = Prompter.Confirm("\nApply configuration and scaffold files now?", true);
            if (!proceed)
            {
                Console.WriteLine("\nSetup aborted by user.");
                return;
            }
        }

        Console.WriteLine("\n\x1b[36mGenerating configuration...\x1b[0m");

        var answers = new SetupAnswers
        {
            ConsumerKey = consumerKey,
            ConsumerSecret = consumerSecret,
            Environment = environment,
            ShortCode = shortCode,
            Passkey = passkey,
            Framework = framework,
            TargetDir = targetDir
        };

        var settingsFile = ProjectScaffolder.UpdateAppSettings(targetDir, answers);
        Console.WriteLine($"  \x1b[32m✔\x1b[0m Configured settings: \x1b[1m{Path.GetFileName(settingsFile)}\x1b[0m");

        var files = ProjectScaffolder.ScaffoldFiles(targetDir, answers);
        foreach (var file in files)
        {
            Console.WriteLine($"  \x1b[32m✔\x1b[0m Generated code file: \x1b[1m{Path.GetRelativePath(targetDir, file)}\x1b[0m");
        }

        Console.WriteLine("\n\x1b[32m=========================================================\x1b[0m");
        Console.WriteLine("\x1b[1;32m  ✔ Setup completed successfully!\x1b[0m");
        Console.WriteLine("\x1b[32m=========================================================\x1b[0m\n");
        Console.WriteLine("Next Steps:");
        Console.WriteLine("1. Ensure your Program.cs calls builder.Services.AddDaraja(builder.Configuration)");
        Console.WriteLine("2. Map the generated endpoints/controller (e.g. app.MapDarajaEndpoints() or app.MapControllers())");
        Console.WriteLine("3. Run \x1b[36msafaricom-daraja test\x1b[0m anytime to verify cryptography, or \x1b[36msafaricom-daraja token\x1b[0m to verify your credentials\n");

        await Task.CompletedTask;
    }

    private static Dictionary<string, string> ParseFlags(string[] args)
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
}
