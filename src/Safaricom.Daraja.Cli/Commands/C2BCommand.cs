using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Services;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Cli.Commands;

public static class C2BCommand
{
    public static async Task ExecuteAsync(string[] args)
    {
        var flags = CliFlags.Parse(args);
        var config = CliConfigLoader.LoadConfig();
        var c2b = new C2BClient(new DarajaTransport(config));

        if (flags.ContainsKey("simulate"))
        {
            await SimulateAsync(c2b, flags);
        }
        else
        {
            await RegisterAsync(c2b, flags);
        }
    }

    private static async Task RegisterAsync(C2BClient c2b, Dictionary<string, string> flags)
    {
        var shortCode = CliFlags.Require(flags, "shortcode", "Usage: safaricom-daraja c2b --shortcode <code> --confirmation-url <url> --validation-url <url> [--response-type Completed|Cancelled]");
        var confirmationUrl = CliFlags.Require(flags, "confirmation-url", "Usage: safaricom-daraja c2b --shortcode <code> --confirmation-url <url> --validation-url <url>");
        var validationUrl = CliFlags.Require(flags, "validation-url", "Usage: safaricom-daraja c2b --shortcode <code> --confirmation-url <url> --validation-url <url>");
        var responseType = flags.GetValueOrDefault("response-type") ?? nameof(C2BResponseType.Completed);

        if (confirmationUrl.Contains("mpesa", StringComparison.OrdinalIgnoreCase) || validationUrl.Contains("mpesa", StringComparison.OrdinalIgnoreCase))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Warning: Daraja rejects ConfirmationURL/ValidationURL values containing the word \"mpesa\" with a 400 - this will likely fail.");
            Console.ResetColor();
        }

        try
        {
            var response = await c2b.RegisterUrlAsync(new C2BRegisterUrlRequest
            {
                ShortCode = shortCode,
                ConfirmationURL = confirmationUrl,
                ValidationURL = validationUrl,
                ResponseType = responseType,
            });

            Console.WriteLine($"ResponseCode: {response.ResponseCode}");
            Console.WriteLine($"ResponseDescription: {response.ResponseDescription}");
            Console.WriteLine($"OriginatorConversationID: {response.OriginatorConversationId}");
        }
        catch (Exception ex)
        {
            CliFlags.Fail($"Error: {ex.Message}");
        }
    }

    private static async Task SimulateAsync(C2BClient c2b, Dictionary<string, string> flags)
    {
        var shortCode = CliFlags.Require(flags, "shortcode", "Usage: safaricom-daraja c2b --simulate --shortcode <code> --phone <msisdn> --amount <n> --bill-ref <ref>");
        var phone = CliFlags.Require(flags, "phone", "Usage: safaricom-daraja c2b --simulate --shortcode <code> --phone <msisdn> --amount <n> --bill-ref <ref>");
        var billRef = CliFlags.Require(flags, "bill-ref", "Usage: safaricom-daraja c2b --simulate --shortcode <code> --phone <msisdn> --amount <n> --bill-ref <ref>");
        var amount = flags.GetValueOrDefault("amount") ?? "1";
        var commandId = flags.GetValueOrDefault("command-id") ?? nameof(C2BCommandId.CustomerPayBillOnline);

        try
        {
            var response = await c2b.SimulateAsync(new C2BSimulateRequest
            {
                ShortCode = shortCode,
                CommandID = commandId,
                Amount = amount,
                Msisdn = phone,
                BillRefNumber = billRef,
            });

            Console.WriteLine($"ConversationID: {response.ConversationId}");
            Console.WriteLine($"OriginatorConversationID: {response.OriginatorConversationId}");
            Console.WriteLine($"ResponseDescription: {response.ResponseDescription}");
        }
        catch (Exception ex)
        {
            CliFlags.Fail($"Error: {ex.Message}");
        }
    }
}
