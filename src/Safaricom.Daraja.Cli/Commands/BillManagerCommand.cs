using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Services;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Cli.Commands;

public static class BillManagerCommand
{
    public static async Task ExecuteAsync(string[] args)
    {
        var flags = CliFlags.Parse(args);
        var config = CliConfigLoader.LoadConfig();
        var billManager = new BillManagerClient(new DarajaTransport(config));

        try
        {
            if (flags.ContainsKey("optin"))
            {
                await OptInAsync(billManager, flags);
            }
            else if (flags.ContainsKey("single-invoice"))
            {
                await SingleInvoiceAsync(billManager, flags);
            }
            else if (flags.ContainsKey("cancel-single-invoice"))
            {
                const string usage = "Usage: safaricom-daraja bill-manager --cancel-single-invoice --reference <external-reference>";
                var reference = CliFlags.Require(flags, "reference", usage);
                var response = await billManager.CancelSingleInvoiceAsync(new BillManagerCancelSingleInvoiceRequest { ExternalReference = reference });
                Print(response);
            }
            else if (flags.ContainsKey("cancel-bulk-invoices"))
            {
                const string usage = "Usage: safaricom-daraja bill-manager --cancel-bulk-invoices --references <ref1,ref2,...>";
                var referencesCsv = CliFlags.Require(flags, "references", usage);
                var references = referencesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                var response = await billManager.CancelBulkInvoicesAsync(new BillManagerCancelBulkInvoicesRequest { ExternalReferences = references });
                Print(response);
            }
            else
            {
                CliFlags.Fail(
                    "Specify an action: --optin, --single-invoice, --cancel-single-invoice, or --cancel-bulk-invoices.\n" +
                    "Bulk invoicing (multiple invoices in one call) isn't supported from the CLI - use the SDK directly for that.");
            }
        }
        catch (Exception ex)
        {
            CliFlags.Fail($"Error: {ex.Message}");
        }
    }

    private static async Task OptInAsync(BillManagerClient billManager, Dictionary<string, string> flags)
    {
        const string usage = "Usage: safaricom-daraja bill-manager --optin --shortcode <code> --email <email> --contact <msisdn> --callback-url <url> [--send-reminders]";
        var shortCode = CliFlags.Require(flags, "shortcode", usage);
        var email = CliFlags.Require(flags, "email", usage);
        var contact = CliFlags.Require(flags, "contact", usage);
        var callbackUrl = CliFlags.Require(flags, "callback-url", usage);

        var response = await billManager.OptInAsync(new BillManagerOptInRequest
        {
            ShortCode = shortCode,
            Email = email,
            OfficialContact = contact,
            SendReminders = flags.ContainsKey("send-reminders") ? 1 : 0,
            CallbackUrl = callbackUrl,
        });

        Print(response);
    }

    private static async Task SingleInvoiceAsync(BillManagerClient billManager, Dictionary<string, string> flags)
    {
        const string usage =
            "Usage: safaricom-daraja bill-manager --single-invoice --reference <external-reference> --name <billed-full-name> " +
            "--phone <billed-msisdn> --period <yyyy-MM-dd> --invoice-name <text> --due-date <yyyy-MM-dd> " +
            "--account-reference <ref> --amount <n> [--item-name <text>]";

        var reference = CliFlags.Require(flags, "reference", usage);
        var name = CliFlags.Require(flags, "name", usage);
        var phone = CliFlags.Require(flags, "phone", usage);
        var period = CliFlags.Require(flags, "period", usage);
        var invoiceName = CliFlags.Require(flags, "invoice-name", usage);
        var dueDate = CliFlags.Require(flags, "due-date", usage);
        var accountReference = CliFlags.Require(flags, "account-reference", usage);
        var amountStr = CliFlags.Require(flags, "amount", usage);
        var amount = decimal.Parse(amountStr);
        var itemName = flags.GetValueOrDefault("item-name") ?? invoiceName;

        var response = await billManager.SendSingleInvoiceAsync(new BillManagerInvoice
        {
            ExternalReference = reference,
            BilledFullName = name,
            BilledPhoneNumber = phone,
            BilledPeriod = period,
            InvoiceName = invoiceName,
            DueDate = dueDate,
            AccountReference = accountReference,
            Amount = amount,
            InvoiceItems = new List<BillManagerInvoiceItem> { new() { ItemName = itemName, Amount = amount } },
        });

        Print(response);
    }

    private static void Print(BillManagerResponse response)
    {
        Console.WriteLine($"ResultCode: {response.ResultCode}");
        Console.WriteLine($"ResultMessage: {response.ResultMessage}");
    }
}
