using System;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Services;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Cli.Commands;

public static class TransactionStatusCommand
{
    private const string Usage =
        "Usage: safaricom-daraja transaction-status --initiator <name> --transaction-id <id> --shortcode <code> " +
        "--remarks <text> --result-url <url> --timeout-url <url> [--occasion <text>] (+ SecurityCredential flags - see --help)";

    public static async Task ExecuteAsync(string[] args)
    {
        var flags = CliFlags.Parse(args);
        var config = CliConfigLoader.LoadConfig();
        var client = new TransactionStatusClient(new DarajaTransport(config));

        var initiator = CliFlags.Require(flags, "initiator", Usage);
        var transactionId = CliFlags.Require(flags, "transaction-id", Usage);
        var shortCode = CliFlags.Require(flags, "shortcode", Usage);
        var remarks = CliFlags.Require(flags, "remarks", Usage);
        var resultUrl = CliFlags.Require(flags, "result-url", Usage);
        var timeoutUrl = CliFlags.Require(flags, "timeout-url", Usage);
        var securityCredential = SecurityCredentialCliHelper.Resolve(flags);

        try
        {
            var response = await client.QueryAsync(new TransactionStatusRequest
            {
                Initiator = initiator,
                SecurityCredential = securityCredential,
                TransactionID = transactionId,
                PartyA = shortCode,
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
