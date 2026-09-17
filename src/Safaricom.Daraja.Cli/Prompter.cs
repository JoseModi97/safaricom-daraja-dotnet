using System;
using System.Collections.Generic;

namespace Safaricom.Daraja.Cli;

public static class Prompter
{
    public static string Ask(string message, string? defaultValue = null, Func<string, bool>? validate = null, string? validationMessage = null)
    {
        while (true)
        {
            Console.Write(!string.IsNullOrEmpty(defaultValue) ? $"{message} [{defaultValue}]: " : $"{message}: ");

            var input = Console.ReadLine();
            var candidate = string.IsNullOrWhiteSpace(input) ? defaultValue ?? string.Empty : input.Trim();

            if (validate == null || validate(candidate))
            {
                return candidate;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(validationMessage ?? "Invalid input. Please try again.");
            Console.ResetColor();
        }
    }

    public static string AskSecret(string message, string? defaultValue = null, Func<string, bool>? validate = null, string? validationMessage = null)
    {
        while (true)
        {
            Console.Write(!string.IsNullOrEmpty(defaultValue) ? $"{message} [****]: " : $"{message}: ");

            var password = ReadPassword();
            var candidate = string.IsNullOrWhiteSpace(password) ? defaultValue ?? string.Empty : password.Trim();

            if (validate == null || validate(candidate))
            {
                return candidate;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(validationMessage ?? "Invalid input. Please try again.");
            Console.ResetColor();
        }
    }

    public static bool Confirm(string message, bool defaultYes = true)
    {
        var hint = defaultYes ? "[Y/n]" : "[y/N]";
        Console.Write($"{message} {hint}: ");
        var input = Console.ReadLine()?.Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(input))
        {
            return defaultYes;
        }

        return input is "y" or "yes" or "true" or "1";
    }

    public static string Select(string message, IReadOnlyList<(string Label, string Value)> choices, int defaultIndex = 0)
    {
        Console.WriteLine(message);
        for (int i = 0; i < choices.Count; i++)
        {
            var isDefault = i == defaultIndex ? " (default)" : "";
            Console.WriteLine($"  {i + 1}) {choices[i].Label}{isDefault}");
        }

        while (true)
        {
            Console.Write($"Enter selection (1-{choices.Count}) [{defaultIndex + 1}]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input))
            {
                return choices[defaultIndex].Value;
            }

            if (int.TryParse(input, out var choice) && choice >= 1 && choice <= choices.Count)
            {
                return choices[choice - 1].Value;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Please enter a number between 1 and {choices.Count}.");
            Console.ResetColor();
        }
    }

    private static string ReadPassword()
    {
        var pass = "";
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }
            if (key.Key == ConsoleKey.Backspace)
            {
                if (pass.Length > 0)
                {
                    pass = pass.Substring(0, pass.Length - 1);
                    Console.Write("\b \b");
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                pass += key.KeyChar;
                Console.Write("*");
            }
        }
        return pass;
    }
}
