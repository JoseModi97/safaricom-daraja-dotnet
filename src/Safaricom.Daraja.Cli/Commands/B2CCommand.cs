using System;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Services;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Cli.Commands;

public static class B2CCommand
{
    private const string Usage =
        "Usage: safaricom-daraja b2c --initiator <name> --shortcode <code> --phone <msisdn> --amount <n> " +
        "--remarks <text> --result-url <url> --timeout-url <url> [--command-id BusinessPayment|SalaryPayment|PromotionPayment] " +
        "[--occasion <text>] (+ SecurityCredential flags - see --help)";

    public static async Task ExecuteAsync(string[] args)
    {
        var flags = CliFlags.Parse(args);
        var config = CliConfigLoader.LoadConfig();
        var b2c = new B2CClient(new DarajaTransport(config));

        var initiator = CliFlags.Require(flags, "initiator", Usage);
        var shortCode = CliFlags.Require(flags, "shortcode", Usage);
        var phone = CliFlags.Require(flags, "phone", Usage);
        var amount = CliFlags.Require(flags, "amount", Usage);
        var remarks = CliFlags.Require(flags, "remarks", Usage);
        var resultUrl = CliFlags.Require(flags, "result-url", Usage);
        var timeoutUrl = CliFlags.Require(flags, "timeout-url", Usage);
        var commandId = flags.GetValueOrDefault("command-id") ?? nameof(B2CCommandId.BusinessPayment);
        var securityCredential = SecurityCredentialCliHelper.Resolve(flags);

        try
        {
            var response = await b2c.SendAsync(new B2CRequest
            {
                InitiatorName = initiator,
                SecurityCredential = securityCredential,
                CommandID = commandId,
                Amount = amount,
                PartyA = shortCode,
                PartyB = phone,
                Remarks = remarks,
                QueueTimeOutURL = timeoutUrl,
                ResultURL = resultUrl,
                Occasion = flags.GetValueOrDefault("occasion"),
            });

            Console.WriteLine($"ResponseCode: {response.ResponseCode}");
            Console.WriteLine($"ResponseDescription: {response.ResponseDescription}");
            Console.WriteLine($"ConversationID: {response.ConversationId}");
            Console.WriteLine($"OriginatorConversationID: {response.OriginatorConversationId}");
            Console.WriteLine("\nThe outcome arrives asynchronously at --result-url - this SDK cannot poll it for you.");
        }
        catch (Exception ex)
        {
            CliFlags.Fail($"Error: {ex.Message}");
        }
    }
}
