using System;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Services;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Cli.Commands;

/// <summary>Lipa na Bonga is not part of Safaricom's public Daraja catalog - see the README's note on it.</summary>
public static class BongaCommand
{
    public static async Task ExecuteAsync(string[] args)
    {
        var flags = CliFlags.Parse(args);
        var config = CliConfigLoader.LoadConfig();
        var bonga = new LipaNaBongaClient(new DarajaTransport(config));

        try
        {
            if (flags.ContainsKey("calculate"))
            {
                const string usage = "Usage: safaricom-daraja bonga --calculate --points <n>";
                var points = CliFlags.Require(flags, "points", usage);

                var result = await bonga.CalculatePointsAsync(new BongaCalculatePointsRequest { Points = points });
                Console.WriteLine(result.Json);
                return;
            }

            const string redeemUsage = "Usage: safaricom-daraja bonga --phone <msisdn> --amount <n> --points <n> --shortcode <code> --account-number <ref> [--conversion-rate <n>]";
            var msisdn = CliFlags.Require(flags, "phone", redeemUsage);
            var amount = CliFlags.Require(flags, "amount", redeemUsage);
            var bongaPoints = CliFlags.Require(flags, "points", redeemUsage);
            var shortCode = CliFlags.Require(flags, "shortcode", redeemUsage);
            var accountNumber = CliFlags.Require(flags, "account-number", redeemUsage);
            var conversionRate = flags.GetValueOrDefault("conversion-rate") ?? "0.2";

            var redeemResult = await bonga.RedeemPaybillAsync(new BongaRedeemPaybillRequest
            {
                Msisdn = msisdn,
                Amount = decimal.Parse(amount),
                BongaPoints = decimal.Parse(bongaPoints),
                ConversionRate = decimal.Parse(conversionRate),
                ShortCode = shortCode,
                AccountNumber = accountNumber,
            });
            Console.WriteLine(redeemResult.Json);
        }
        catch (Exception ex)
        {
            CliFlags.Fail($"Error: {ex.Message}");
        }
    }
}
