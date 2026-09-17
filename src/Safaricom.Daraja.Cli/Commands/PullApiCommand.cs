using System;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Services;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Cli.Commands;

public static class PullApiCommand
{
    public static async Task ExecuteAsync(string[] args)
    {
        var flags = CliFlags.Parse(args);
        var config = CliConfigLoader.LoadConfig();
        var pullApi = new PullApiClient(new DarajaTransport(config));

        try
        {
            if (flags.ContainsKey("register"))
            {
                const string usage = "Usage: safaricom-daraja pull-api --register --shortcode <code> --nominated-number <msisdn> --callback-url <url> [--request-type Paybill|Till]";
                var shortCode = CliFlags.Require(flags, "shortcode", usage);
                var nominatedNumber = CliFlags.Require(flags, "nominated-number", usage);
                var callbackUrl = CliFlags.Require(flags, "callback-url", usage);
                var requestType = flags.GetValueOrDefault("request-type") ?? nameof(PullApiRequestType.Paybill);

                var response = await pullApi.RegisterAsync(new PullApiRegisterRequest
                {
                    ShortCode = shortCode,
                    RequestType = requestType,
                    NominatedNumber = nominatedNumber,
                    CallBackURL = callbackUrl,
                });

                Console.WriteLine($"ResponseCode: {response.ResponseCode}");
                Console.WriteLine($"ResponseMessage: {response.ResponseMessage}");
                return;
            }

            const string queryUsage = "Usage: safaricom-daraja pull-api --shortcode <code> --start-date \"yyyy-MM-dd HH:mm:ss\" --end-date \"yyyy-MM-dd HH:mm:ss\" [--offset <n>]";
            var qShortCode = CliFlags.Require(flags, "shortcode", queryUsage);
            var startDate = CliFlags.Require(flags, "start-date", queryUsage);
            var endDate = CliFlags.Require(flags, "end-date", queryUsage);
            var offset = flags.GetValueOrDefault("offset") ?? "0";

            var query = await pullApi.QueryAsync(new PullApiQueryRequest
            {
                ShortCode = qShortCode,
                StartDate = startDate,
                EndDate = endDate,
                OffSetValue = offset,
            });

            Console.WriteLine($"ResponseCode: {query.ResponseCode}");
            Console.WriteLine($"ResponseMessage: {query.ResponseMessage}");
            Console.WriteLine($"Transactions returned: {query.Response?.Count ?? 0}");
            foreach (var txn in query.Response ?? new())
            {
                Console.WriteLine($"  {txn.TransTime} | {txn.TransId} | {txn.TransAmount} | {txn.Msisdn} | {txn.BillRefNumber}");
            }
        }
        catch (Exception ex)
        {
            CliFlags.Fail($"Error: {ex.Message}");
        }
    }
}
