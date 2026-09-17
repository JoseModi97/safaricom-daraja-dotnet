using System;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Services;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Cli.Commands;

public static class ReversalCommand
{
    private const string Usage =
        "Usage: safaricom-daraja reversal --initiator <name> --transaction-id <id> --amount <n> " +
        "--receiver-party <shortcode> --remarks <text> --result-url <url> --timeout-url <url> " +
        "[--occasion <text>] (+ SecurityCredential flags - see --help)";

    public static async Task ExecuteAsync(string[] args)
    {
        var flags = CliFlags.Parse(args);
        var config = CliConfigLoader.LoadConfig();
        var reversal = new ReversalClient(new DarajaTransport(config));

        var initiator = CliFlags.Require(flags, "initiator", Usage);
        var transactionId = CliFlags.Require(flags, "transaction-id", Usage);
        var amount = CliFlags.Require(flags, "amount", Usage);
        var receiverParty = CliFlags.Require(flags, "receiver-party", Usage);
        var remarks = CliFlags.Require(flags, "remarks", Usage);
        var resultUrl = CliFlags.Require(flags, "result-url", Usage);
        var timeoutUrl = CliFlags.Require(flags, "timeout-url", Usage);
        var securityCredential = SecurityCredentialCliHelper.Resolve(flags);

        try
        {
            var response = await reversal.SendAsync(new ReversalRequest
            {
                Initiator = initiator,
                SecurityCredential = securityCredential,
                TransactionID = transactionId,
                Amount = amount,
                ReceiverParty = receiverParty,
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
