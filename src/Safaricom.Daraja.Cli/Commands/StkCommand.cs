using System;
using System.Globalization;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Services;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Cli.Commands;

public static class StkCommand
{
    public static async Task ExecuteAsync(string[] args)
    {
        var flags = CliFlags.Parse(args);
        var config = CliConfigLoader.LoadConfig();
        var stkPush = new StkPushClient(new DarajaTransport(config));

        var shortCode = flags.GetValueOrDefault("shortcode");
        var passkey = flags.GetValueOrDefault("passkey");

        if (string.IsNullOrWhiteSpace(shortCode) || string.IsNullOrWhiteSpace(passkey))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Missing required --shortcode and/or --passkey\n");
            Console.ResetColor();
            Program.PrintHelp();
            Environment.Exit(1);
            return;
        }

        var checkoutRequestId = flags.GetValueOrDefault("query");
        if (!string.IsNullOrWhiteSpace(checkoutRequestId))
        {
            await QueryAsync(stkPush, shortCode!, passkey!, checkoutRequestId!);
            return;
        }

        var phone = flags.GetValueOrDefault("phone");
        var callbackUrl = flags.GetValueOrDefault("callback-url");
        if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(callbackUrl))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Missing required --phone and/or --callback-url (or pass --query <CheckoutRequestID> to check an existing push)\n");
            Console.ResetColor();
            Program.PrintHelp();
            Environment.Exit(1);
            return;
        }

        var amountStr = flags.GetValueOrDefault("amount") ?? "1";
        if (!decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
        {
            amount = 1;
        }

        var accountReference = flags.GetValueOrDefault("account-reference") ?? $"CLI-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var description = flags.GetValueOrDefault("description") ?? "Payment";

        try
        {
            Console.WriteLine($"\x1b[36mRaising STK Push on {config.Environment}...\x1b[0m\n");

            var response = await stkPush.PushAsync(new StkPushRequest
            {
                ShortCode = shortCode!,
                Passkey = passkey!,
                TransactionType = StkTransactionType.CustomerPayBillOnline,
                Amount = amount,
                PhoneNumber = phone!,
                CallBackURL = callbackUrl!,
                AccountReference = accountReference,
                TransactionDesc = description,
            });

            Console.WriteLine($"  ResponseCode: {response.ResponseCode}");
            Console.WriteLine($"  ResponseDescription: {response.ResponseDescription}");
            Console.WriteLine($"  CustomerMessage: {response.CustomerMessage}");
            Console.WriteLine($"  MerchantRequestID: {response.MerchantRequestId}");
            Console.WriteLine($"  CheckoutRequestID: {response.CheckoutRequestId}");

            if (response.IsAccepted)
            {
                Console.WriteLine($"\n\x1b[32mPrompt sent. Check the phone, then run:\x1b[0m");
                Console.WriteLine($"  safaricom-daraja stk --shortcode {shortCode} --passkey **** --query {response.CheckoutRequestId}\n");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nError: {ex.Message}");
            Console.ResetColor();
            Environment.Exit(1);
        }
    }

    private static async Task QueryAsync(StkPushClient stkPush, string shortCode, string passkey, string checkoutRequestId)
    {
        try
        {
            Console.WriteLine($"\x1b[36mQuerying CheckoutRequestID {checkoutRequestId}...\x1b[0m\n");

            var status = await stkPush.QueryAsync(new StkPushQueryRequest
            {
                ShortCode = shortCode,
                Passkey = passkey,
                CheckoutRequestId = checkoutRequestId,
            });

            Console.WriteLine($"  ResponseCode: {status.ResponseCode}");
            Console.WriteLine($"  ResponseDescription: {status.ResponseDescription}");
            Console.WriteLine($"  ResultCode: {status.ResultCode}");
            Console.WriteLine($"  ResultDesc: {status.ResultDesc}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nError: {ex.Message}");
            Console.ResetColor();
            Environment.Exit(1);
        }
    }
}
