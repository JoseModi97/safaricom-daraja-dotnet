using Safaricom.Daraja;
using Safaricom.Daraja.Models;

// Credentials come from environment variables - never hardcode real credentials in source:
//   DARAJA_CONSUMER_KEY, DARAJA_CONSUMER_SECRET   (read automatically by DarajaConfig)
//   DARAJA_SHORTCODE, DARAJA_PASSKEY              (this script's own extra inputs)
var shortCode = Environment.GetEnvironmentVariable("DARAJA_SHORTCODE") ?? "174379";
var passkey = Environment.GetEnvironmentVariable("DARAJA_PASSKEY")
    ?? throw new InvalidOperationException("Set DARAJA_PASSKEY (and DARAJA_CONSUMER_KEY / DARAJA_CONSUMER_SECRET) before running this example.");

// DarajaConfig() with no arguments reads DARAJA_CONSUMER_KEY / DARAJA_CONSUMER_SECRET /
// DARAJA_ENVIRONMENT from the environment automatically.
var client = new DarajaClient(new DarajaConfig());

Console.WriteLine($"Raising an STK Push on {client.Config.Environment} ({client.Config.GetEffectiveBaseAddress()})...");

var push = await client.StkPush.PushAsync(new StkPushRequest
{
    ShortCode = shortCode,
    Passkey = passkey,
    TransactionType = StkTransactionType.CustomerPayBillOnline,
    Amount = 1,
    // 254708374149 is Safaricom's standard published Sandbox test MSISDN - it accepts STK
    // pushes without needing a real device. Swap in a real number to test on your own phone.
    PhoneNumber = "254708374149",
    CallBackURL = "https://example.com/mpesa/stk-callback", // Sandbox doesn't require this to be reachable
    AccountReference = $"INV-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
    TransactionDesc = "Example payment",
});

Console.WriteLine($"  ResponseCode: {push.ResponseCode}");
Console.WriteLine($"  ResponseDescription: {push.ResponseDescription}");
Console.WriteLine($"  CustomerMessage: {push.CustomerMessage}");
Console.WriteLine($"  CheckoutRequestID: {push.CheckoutRequestId}");

if (!push.IsAccepted)
{
    Console.WriteLine("\nDaraja did not accept the push - stopping before querying.");
    return;
}

Console.WriteLine("\nWaiting 10 seconds before querying the outcome...");
await Task.Delay(TimeSpan.FromSeconds(10));

var status = await client.StkPush.QueryAsync(new StkPushQueryRequest
{
    ShortCode = shortCode,
    Passkey = passkey,
    CheckoutRequestId = push.CheckoutRequestId!,
});

Console.WriteLine($"\n  ResponseCode: {status.ResponseCode}");
Console.WriteLine($"  ResponseDescription: {status.ResponseDescription}");
Console.WriteLine($"  ResultCode: {status.ResultCode}");
Console.WriteLine($"  ResultDesc: {status.ResultDesc}");
