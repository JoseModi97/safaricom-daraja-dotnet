using System;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Services;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Cli.Commands;

public static class RatibaCommand
{
    private const string Usage =
        "Usage: safaricom-daraja ratiba --name <text> --shortcode <code> --phone <msisdn> --amount <n> " +
        "--callback-url <url> --account-reference <ref> --start-date <yyyy-MM-dd> --end-date <yyyy-MM-dd> " +
        "[--frequency Monthly|Weekly|Daily|...] [--receiver-type Paybill|BuyGoods] [--description <text>]";

    public static async Task ExecuteAsync(string[] args)
    {
        var flags = CliFlags.Parse(args);
        var config = CliConfigLoader.LoadConfig();
        var ratiba = new RatibaClient(new DarajaTransport(config));

        var name = CliFlags.Require(flags, "name", Usage);
        var shortCode = CliFlags.Require(flags, "shortcode", Usage);
        var phone = CliFlags.Require(flags, "phone", Usage);
        var amount = CliFlags.Require(flags, "amount", Usage);
        var callbackUrl = CliFlags.Require(flags, "callback-url", Usage);
        var accountReference = CliFlags.Require(flags, "account-reference", Usage);
        var startDate = CliFlags.Require(flags, "start-date", Usage);
        var endDate = CliFlags.Require(flags, "end-date", Usage);

        var receiverType = flags.GetValueOrDefault("receiver-type") ?? nameof(RatibaReceiverType.Paybill);
        var receiverTypeValue = Enum.TryParse<RatibaReceiverType>(receiverType, true, out var parsedReceiver)
            ? ((int)parsedReceiver).ToString()
            : ((int)RatibaReceiverType.Paybill).ToString();

        var frequencyName = flags.GetValueOrDefault("frequency") ?? nameof(RatibaFrequency.Monthly);
        var frequencyValue = Enum.TryParse<RatibaFrequency>(frequencyName, true, out var parsedFrequency)
            ? ((int)parsedFrequency).ToString()
            : ((int)RatibaFrequency.Monthly).ToString();

        try
        {
            var response = await ratiba.CreateAsync(new RatibaCreateRequest
            {
                StandingOrderName = name,
                BusinessShortCode = shortCode,
                TransactionType = receiverType.Equals(nameof(RatibaReceiverType.BuyGoods), StringComparison.OrdinalIgnoreCase)
                    ? "Standing Order Customer Pay Merchant"
                    : "Standing Order Customer Pay Bill",
                Amount = amount,
                PartyA = phone,
                ReceiverPartyIdentifierType = receiverTypeValue,
                CallBackURL = callbackUrl,
                AccountReference = accountReference,
                TransactionDesc = flags.GetValueOrDefault("description") ?? "Standing order",
                Frequency = frequencyValue,
                StartDate = startDate,
                EndDate = endDate,
            });

            Console.WriteLine($"ResponseCode: {response.ResponseCode}");
            Console.WriteLine($"ResponseDescription: {response.ResponseDescription}");
        }
        catch (Exception ex)
        {
            CliFlags.Fail($"Error: {ex.Message}");
        }
    }
}
