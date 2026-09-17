using System;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Services;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Cli.Commands;

public static class B2BCommand
{
    private const string Usage =
        "Usage:\n" +
        "  safaricom-daraja b2b --initiator <name> --shortcode <code> --party-b <shortcode> --amount <n> " +
        "--account-reference <ref> --remarks <text> --result-url <url> --timeout-url <url> " +
        "[--command-id BusinessPayBill|BusinessBuyGoods|...] (+ SecurityCredential flags)\n" +
        "  safaricom-daraja b2b --pay-taxes --initiator <name> --shortcode <code> --kra-shortcode <code> " +
        "--prn <registration-number> --amount <n> --remarks <text> --result-url <url> --timeout-url <url>";

    public static async Task ExecuteAsync(string[] args)
    {
        var flags = CliFlags.Parse(args);
        var config = CliConfigLoader.LoadConfig();
        var b2b = new B2BClient(new DarajaTransport(config));

        var initiator = CliFlags.Require(flags, "initiator", Usage);
        var shortCode = CliFlags.Require(flags, "shortcode", Usage);
        var amount = CliFlags.Require(flags, "amount", Usage);
        var remarks = CliFlags.Require(flags, "remarks", Usage);
        var resultUrl = CliFlags.Require(flags, "result-url", Usage);
        var timeoutUrl = CliFlags.Require(flags, "timeout-url", Usage);
        var securityCredential = SecurityCredentialCliHelper.Resolve(flags);

        try
        {
            if (flags.ContainsKey("pay-taxes"))
            {
                var kraShortCode = CliFlags.Require(flags, "kra-shortcode", Usage);
                var prn = CliFlags.Require(flags, "prn", Usage);

                var response = await b2b.PayTaxesAsync(
                    initiator: initiator,
                    securityCredential: securityCredential,
                    businessShortCode: shortCode,
                    kraShortCode: kraShortCode,
                    paymentRegistrationNumber: prn,
                    amount: decimal.Parse(amount),
                    remarks: remarks,
                    queueTimeOutUrl: timeoutUrl,
                    resultUrl: resultUrl);

                PrintResponse(response);
                return;
            }

            var partyB = CliFlags.Require(flags, "party-b", Usage);
            var accountReference = CliFlags.Require(flags, "account-reference", Usage);
            var commandId = flags.GetValueOrDefault("command-id") ?? nameof(B2BCommandId.BusinessPayBill);

            var ack = await b2b.SendAsync(new B2BRequest
            {
                Initiator = initiator,
                SecurityCredential = securityCredential,
                CommandID = commandId,
                Amount = amount,
                PartyA = shortCode,
                PartyB = partyB,
                AccountReference = accountReference,
                Remarks = remarks,
                QueueTimeOutURL = timeoutUrl,
                ResultURL = resultUrl,
            });

            PrintResponse(ack);
        }
        catch (Exception ex)
        {
            CliFlags.Fail($"Error: {ex.Message}");
        }
    }

    private static void PrintResponse(DarajaAckResponse response)
    {
        Console.WriteLine($"ResponseCode: {response.ResponseCode}");
        Console.WriteLine($"ResponseDescription: {response.ResponseDescription}");
        Console.WriteLine($"ConversationID: {response.ConversationId}");
        Console.WriteLine($"OriginatorConversationID: {response.OriginatorConversationId}");
        Console.WriteLine("\nThe outcome arrives asynchronously at --result-url - this SDK cannot poll it for you.");
    }
}
