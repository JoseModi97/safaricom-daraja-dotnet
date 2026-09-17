using System;
using System.IO;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Services;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Cli.Commands;

public static class QrCommand
{
    private const string Usage =
        "Usage: safaricom-daraja qr --merchant-name <name> --reference-no <ref> --amount <n> --cpi <shortcode-or-till> " +
        "[--transaction-code PayBill|BuyGoods|WithdrawAtAgent|SendMoney|SendToBusiness] [--size <px>] [--output <file.png>]";

    public static async Task ExecuteAsync(string[] args)
    {
        var flags = CliFlags.Parse(args);
        var config = CliConfigLoader.LoadConfig();
        var qr = new QrCodeClient(new DarajaTransport(config));

        var merchantName = CliFlags.Require(flags, "merchant-name", Usage);
        var referenceNo = CliFlags.Require(flags, "reference-no", Usage);
        var amountStr = CliFlags.Require(flags, "amount", Usage);
        var cpi = CliFlags.Require(flags, "cpi", Usage);

        var transactionCodeName = flags.GetValueOrDefault("transaction-code") ?? nameof(QrTransactionCode.PayBill);
        if (!Enum.TryParse<QrTransactionCode>(transactionCodeName, true, out var transactionCode))
        {
            transactionCode = QrTransactionCode.PayBill;
        }

        var size = int.TryParse(flags.GetValueOrDefault("size"), out var parsedSize) ? parsedSize : 300;

        try
        {
            var response = await qr.GenerateAsync(new QrCodeRequest
            {
                MerchantName = merchantName,
                ReferenceNo = referenceNo,
                Amount = decimal.Parse(amountStr),
                TransactionCode = transactionCode,
                CreditPartyIdentifier = cpi,
                Size = size,
            });

            Console.WriteLine($"ResponseCode: {response.ResponseCode}");
            Console.WriteLine($"ResponseDescription: {response.ResponseDescription}");

            var outputPath = flags.GetValueOrDefault("output");
            if (!string.IsNullOrWhiteSpace(outputPath))
            {
                File.WriteAllBytes(outputPath, response.ToPngBytes());
                Console.WriteLine($"Saved QR image to {outputPath}");
            }
            else if (!string.IsNullOrEmpty(response.QrCodeBase64))
            {
                Console.WriteLine("Pass --output <file.png> to save the QR image; raw base64 below:");
                Console.WriteLine(response.QrCodeBase64);
            }
        }
        catch (Exception ex)
        {
            CliFlags.Fail($"Error: {ex.Message}");
        }
    }
}
