using System;
using System.Reflection;
using System.Threading.Tasks;
using Safaricom.Daraja.Cli.Commands;

namespace Safaricom.Daraja.Cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var command = args.Length > 0 ? args[0] : "init";
        var restArgs = args.Length > 1 ? args[1..] : Array.Empty<string>();

        if (command is "--help" or "-h" or "help")
        {
            PrintHelp();
            return 0;
        }

        if (command is "--version" or "-v" or "version")
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.1.0";
            Console.WriteLine($"safaricom-daraja v{version}");
            return 0;
        }

        PrintBanner();

        try
        {
            switch (command.ToLowerInvariant())
            {
                case "init":
                    await InitCommand.ExecuteAsync(restArgs);
                    return 0;

                case "token":
                    await TokenCommand.ExecuteAsync(restArgs);
                    return 0;

                case "stk":
                    await StkCommand.ExecuteAsync(restArgs);
                    return 0;

                case "c2b":
                    await C2BCommand.ExecuteAsync(restArgs);
                    return 0;

                case "b2c":
                    await B2CCommand.ExecuteAsync(restArgs);
                    return 0;

                case "b2b":
                    await B2BCommand.ExecuteAsync(restArgs);
                    return 0;

                case "reversal":
                    await ReversalCommand.ExecuteAsync(restArgs);
                    return 0;

                case "transaction-status":
                    await TransactionStatusCommand.ExecuteAsync(restArgs);
                    return 0;

                case "account-balance":
                    await AccountBalanceCommand.ExecuteAsync(restArgs);
                    return 0;

                case "ratiba":
                    await RatibaCommand.ExecuteAsync(restArgs);
                    return 0;

                case "bonga":
                    await BongaCommand.ExecuteAsync(restArgs);
                    return 0;

                case "pull-api":
                    await PullApiCommand.ExecuteAsync(restArgs);
                    return 0;

                case "qr":
                    await QrCommand.ExecuteAsync(restArgs);
                    return 0;

                case "bill-manager":
                    await BillManagerCommand.ExecuteAsync(restArgs);
                    return 0;

                case "test":
                    await TestCommand.ExecuteAsync(restArgs);
                    return 0;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Unknown command: {command}\n");
                    Console.ResetColor();
                    PrintHelp();
                    return 1;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nError: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }

    public static void PrintBanner()
    {
        Console.WriteLine("\x1b[32m=========================================================\x1b[0m");
        Console.WriteLine("\x1b[1;32m      Safaricom Daraja SDK Setup Wizard (CLI)            \x1b[0m");
        Console.WriteLine("\x1b[32m=========================================================\x1b[0m");
        Console.WriteLine("Interactive configurator for ASP.NET Core Minimal APIs & MVC\n");
    }

    public static void PrintHelp()
    {
        Console.WriteLine(@"
Usage:
  safaricom-daraja [command] [options]
  dotnet safaricom-daraja [command] [options]

Commands:
  init                Interactive setup wizard to configure credentials & scaffold sample code (Default)
  token               Fetch and print an OAuth access token - quick sanity check of your credentials
  stk                 Raise an STK Push prompt, or query the outcome of a previous one
  c2b                 Register C2B callback URLs, or (Sandbox only) simulate an incoming payment
  b2c                 Pay out from a business shortcode to a customer MSISDN
  b2b                 Move funds between shortcodes, or remit taxes to KRA (--pay-taxes)
  reversal            Reverse a previous M-Pesa transaction
  transaction-status  Query the status of a previous M-Pesa transaction
  account-balance     Query a shortcode's account balance
  ratiba              Create an M-Pesa Ratiba standing order
  bonga               Redeem Bonga points against a payment, or calculate their value
  pull-api            Register a shortcode for the Pull Transactions API, or query it
  qr                  Generate a Dynamic QR code
  bill-manager        Opt in, raise, or cancel a Bill Manager invoice
  test                Run offline cryptography self-checks (SecurityCredential RSA round-trip, STK password format)
  help, --help        Show this help message
  --version, -v       Show version

SecurityCredential flags (b2c, b2b, reversal, transaction-status, account-balance):
  --security-credential <value>           a precomputed SecurityCredential, or
  --cert <path> --initiator-password <p>  a certificate file + password to encrypt now
  (This SDK does not bundle Safaricom's certificate - see the README's SecurityCredential section.)

Options for 'init':
  --consumer-key <key>       Daraja app Consumer Key
  --consumer-secret <secret> Daraja app Consumer Secret
  --environment <env>        Sandbox | Production (default: Sandbox)
  --shortcode <code>         Default Paybill/Till shortcode used in scaffolded samples
  --passkey <key>            Lipa na M-Pesa Online passkey used in scaffolded samples
  --framework <name>         Target: minimal | mvc | console
  --yes, -y                  Skip prompts and use defaults or provided flags

Options for 'token':
  (credentials read from appsettings / DARAJA_* env vars)

Options for 'stk':
  --shortcode <code>      Paybill/Till shortcode (required)
  --passkey <key>         Lipa na M-Pesa Online passkey (required)
  --phone <msisdn>        Payer's phone number (required to push; e.g. 254712345678)
  --amount <n>            Amount to charge (default: 1)
  --callback-url <url>    Your STK callback URL (required to push)
  --account-reference <r> Account reference (default: auto-generated)
  --description <text>    Transaction description (default: ""Payment"")
  --query <id>            Query the outcome of an existing CheckoutRequestID instead of pushing

Options for 'c2b':
  --shortcode <code>            Shortcode (required)
  --confirmation-url <url>      ConfirmationURL to register (register mode)
  --validation-url <url>        ValidationURL to register (register mode)
  --response-type <type>        Completed | Cancelled (default: Completed)
  --simulate                    Switch to simulate mode (Sandbox only)
  --phone <msisdn>               Payer's phone number (simulate mode)
  --amount <n>                   Amount (simulate mode, default: 1)
  --bill-ref <ref>                Bill reference number (simulate mode)
  --command-id <id>              CustomerPayBillOnline | CustomerBuyGoodsOnline (simulate mode)

Options for 'b2c':
  --initiator <name>       API operator username (required)
  --shortcode <code>       Paying business shortcode (required)
  --phone <msisdn>         Receiving customer MSISDN (required)
  --amount <n>             Amount (required)
  --remarks <text>         Remarks (required)
  --result-url <url>       ResultURL (required)
  --timeout-url <url>      QueueTimeOutURL (required)
  --command-id <id>        BusinessPayment | SalaryPayment | PromotionPayment (default: BusinessPayment)
  --occasion <text>        Optional occasion text

Options for 'b2b':
  --initiator <name>          API operator username (required)
  --shortcode <code>          Paying business shortcode (required)
  --party-b <code>            Receiving business shortcode (required, unless --pay-taxes)
  --account-reference <ref>   Account reference (required, unless --pay-taxes)
  --amount <n>                Amount (required)
  --remarks <text>            Remarks (required)
  --result-url <url>          ResultURL (required)
  --timeout-url <url>         QueueTimeOutURL (required)
  --command-id <id>           BusinessPayBill | BusinessBuyGoods | ... (default: BusinessPayBill)
  --pay-taxes                 Switch to Tax Remittance mode
  --kra-shortcode <code>      KRA's receiving shortcode (--pay-taxes mode)
  --prn <number>               KRA Payment Registration Number (--pay-taxes mode)

Options for 'reversal':
  --initiator <name>        API operator username (required)
  --transaction-id <id>     Transaction to reverse (required)
  --amount <n>              Amount (required)
  --receiver-party <code>   Receiving party (required)
  --remarks <text>          Remarks (required)
  --result-url <url>        ResultURL (required)
  --timeout-url <url>       QueueTimeOutURL (required)
  --occasion <text>         Optional occasion text

Options for 'transaction-status':
  --initiator <name>        API operator username (required)
  --transaction-id <id>     Transaction to query (required)
  --shortcode <code>        PartyA (required)
  --remarks <text>          Remarks (required)
  --result-url <url>        ResultURL (required)
  --timeout-url <url>       QueueTimeOutURL (required)
  --occasion <text>         Optional occasion text

Options for 'account-balance':
  --initiator <name>        API operator username (required)
  --shortcode <code>        PartyA (required)
  --remarks <text>          Remarks (required)
  --result-url <url>        ResultURL (required)
  --timeout-url <url>       QueueTimeOutURL (required)

Options for 'ratiba':
  --name <text>                 Standing order name (required)
  --shortcode <code>            Business shortcode (required)
  --phone <msisdn>              Customer MSISDN authorizing the order (required)
  --amount <n>                  Amount per cycle (required)
  --callback-url <url>          CallBackURL (required)
  --account-reference <ref>     Account reference (required)
  --start-date <yyyy-MM-dd>     Start date (required)
  --end-date <yyyy-MM-dd>       End date (required)
  --receiver-type <type>        Paybill | BuyGoods (default: Paybill)
  --frequency <type>            OneOff | Daily | Weekly | Monthly | ... (default: Monthly)
  --description <text>          Transaction description

Options for 'bonga':
  --phone <msisdn>          Payer's phone number (redeem mode, required)
  --amount <n>              Amount (redeem mode, required)
  --points <n>              Bonga points (required in both modes)
  --shortcode <code>        Shortcode (redeem mode, required)
  --account-number <ref>    Account number (redeem mode, required)
  --conversion-rate <n>     Points-to-currency rate (redeem mode, default: 0.2)
  --calculate               Switch to calculate-points mode (only needs --points)

Options for 'pull-api':
  --register                     Switch to register mode
  --shortcode <code>              Shortcode (required)
  --nominated-number <msisdn>     Nominated number (register mode, required)
  --callback-url <url>            CallBackURL (register mode, required)
  --request-type <type>           Paybill | Till (register mode, default: Paybill)
  --start-date ""yyyy-MM-dd HH:mm:ss""  Query start (query mode, required)
  --end-date ""yyyy-MM-dd HH:mm:ss""    Query end (query mode, required)
  --offset <n>                     Paging offset (query mode, default: 0)

Options for 'qr':
  --merchant-name <name>     Merchant name (required)
  --reference-no <ref>       Reference number (required)
  --amount <n>               Amount (required)
  --cpi <code>               Credit Party Identifier: till/paybill/agent number (required)
  --transaction-code <type>  PayBill | BuyGoods | WithdrawAtAgent | SendMoney | SendToBusiness (default: PayBill)
  --size <px>                QR image size in pixels (default: 300)
  --output <file.png>        Save the decoded QR image to this file

Options for 'bill-manager':
  --optin                          Opt a shortcode into Bill Manager
  --shortcode <code>                (optin mode, required)
  --email <email>                   (optin mode, required)
  --contact <msisdn>                 (optin mode, required)
  --callback-url <url>               (optin mode, required)
  --send-reminders                   (optin mode, opt-in to SMS reminders)
  --single-invoice                  Raise a single invoice
  --reference <ref>                  External reference (single-invoice / cancel-single-invoice, required)
  --name <text>                      Billed full name (single-invoice, required)
  --phone <msisdn>                    Billed phone number (single-invoice, required)
  --period <yyyy-MM-dd>               Billed period (single-invoice, required)
  --invoice-name <text>              Invoice name (single-invoice, required)
  --due-date <yyyy-MM-dd>            Due date (single-invoice, required)
  --account-reference <ref>          Account reference (single-invoice, required)
  --amount <n>                       Amount (single-invoice, required)
  --item-name <text>                  Line item name (single-invoice, default: invoice name)
  --cancel-single-invoice           Cancel one invoice (needs --reference)
  --cancel-bulk-invoices            Cancel several invoices (needs --references ref1,ref2,...)
  (Bulk invoicing - multiple invoices in one call - isn't supported from the CLI; use the SDK directly.)
");
    }
}
