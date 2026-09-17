using Safaricom.Daraja;
using Safaricom.Daraja.Exceptions;
using Safaricom.Daraja.Models;

// This sample exercises the SDK against the real Daraja Sandbox using credentials from
// environment variables: DARAJA_CONSUMER_KEY, DARAJA_CONSUMER_SECRET, DARAJA_SHORTCODE,
// DARAJA_PASSKEY. Never hardcode real credentials here.
var shortCode = Environment.GetEnvironmentVariable("DARAJA_SHORTCODE") ?? "174379";
var passkey = Environment.GetEnvironmentVariable("DARAJA_PASSKEY")
    ?? throw new InvalidOperationException("Set DARAJA_CONSUMER_KEY, DARAJA_CONSUMER_SECRET, DARAJA_SHORTCODE and DARAJA_PASSKEY before running this sample.");

var client = new DarajaClient(new DarajaConfig());

Console.WriteLine("=== Safaricom.Daraja Sandbox Sample ===\n");
Console.WriteLine($"Environment: {client.Config.Environment}");
Console.WriteLine($"Base address: {client.Config.GetEffectiveBaseAddress()}\n");

// 1. OAuth
Console.WriteLine("--- 1. OAuth token ---");
string? token = null;
try
{
    token = await client.Auth.GetValidAccessTokenAsync();
    Console.WriteLine($"Access token acquired: {token[..Math.Min(8, token.Length)]}...\n");
}
catch (DarajaApiException ex)
{
    Console.WriteLine($"FAILED (status {ex.StatusCode}): {ex.Message}\n");
    Console.WriteLine("Cannot continue without a token - check DARAJA_CONSUMER_KEY / DARAJA_CONSUMER_SECRET.");
    return;
}

// 2. Dynamic QR Code (no SecurityCredential needed - good smoke test beyond STK/auth)
Console.WriteLine("--- 2. Dynamic QR Code ---");
try
{
    var qr = await client.QrCode.GenerateAsync(new QrCodeRequest
    {
        MerchantName = "Safaricom.Daraja Sample",
        ReferenceNo = $"INV-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
        Amount = 1,
        TransactionCode = QrTransactionCode.PayBill,
        CreditPartyIdentifier = shortCode,
    });
    Console.WriteLine($"ResponseCode: {qr.ResponseCode}, ResponseDescription: {qr.ResponseDescription}");
    Console.WriteLine($"QR image bytes: {qr.ToPngBytes().Length}\n");
}
catch (DarajaApiException ex)
{
    Console.WriteLine($"FAILED (status {ex.StatusCode}): {ex.Message[..Math.Min(200, ex.Message.Length)]}");
    Console.WriteLine("(Continuing - this may be a Sandbox-side outage on this specific endpoint, not necessarily an SDK issue.)\n");
}

// 3. STK Push
Console.WriteLine("--- 3. STK Push ---");
StkPushResponse? push = null;
try
{
    push = await client.StkPush.PushAsync(new StkPushRequest
    {
        ShortCode = shortCode,
        Passkey = passkey,
        TransactionType = StkTransactionType.CustomerPayBillOnline,
        Amount = 1,
        // Safaricom's published Sandbox test MSISDN - accepts pushes without a real device.
        PhoneNumber = "254708374149",
        CallBackURL = "https://example.com/mpesa/stk-callback",
        AccountReference = $"INV-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
        TransactionDesc = "Sample payment",
    });
    Console.WriteLine($"ResponseCode: {push.ResponseCode}, ResponseDescription: {push.ResponseDescription}");
    Console.WriteLine($"CustomerMessage: {push.CustomerMessage}");
    Console.WriteLine($"CheckoutRequestID: {push.CheckoutRequestId}\n");
}
catch (DarajaApiException ex)
{
    Console.WriteLine($"FAILED (status {ex.StatusCode}): {ex.Message}\n");
}

if (push is not { IsAccepted: true })
{
    Console.WriteLine("Skipping query step (push was not accepted or failed).");
    Console.WriteLine("\n=== Done ===");
    return;
}

// 4. STK Query
Console.WriteLine("--- 4. STK Query (waiting 10s first) ---");
await Task.Delay(TimeSpan.FromSeconds(10));

try
{
    var status = await client.StkPush.QueryAsync(new StkPushQueryRequest
    {
        ShortCode = shortCode,
        Passkey = passkey,
        CheckoutRequestId = push.CheckoutRequestId!,
    });
    Console.WriteLine($"ResponseCode: {status.ResponseCode}, ResponseDescription: {status.ResponseDescription}");
    Console.WriteLine($"ResultCode: {status.ResultCode}, ResultDesc: {status.ResultDesc}");
}
catch (DarajaApiException ex)
{
    Console.WriteLine($"FAILED (status {ex.StatusCode}): {ex.Message}");
}

// 5. C2B: register callback URLs, then (Sandbox only) simulate an incoming payment
Console.WriteLine("\n--- 5. C2B Register URL ---");
try
{
    // Note: Daraja rejects ValidationURL/ConfirmationURL values containing the word "mpesa"
    // with a 400 ("Bad Request - Invalid ValidationURL - URL has the word MPESA") - avoid it here.
    var register = await client.C2B.RegisterUrlAsync(new C2BRegisterUrlRequest
    {
        ShortCode = shortCode,
        ResponseType = nameof(C2BResponseType.Completed),
        ConfirmationURL = "https://example.com/payments/c2b/confirmation",
        ValidationURL = "https://example.com/payments/c2b/validation",
    });
    Console.WriteLine($"ResponseCode: {register.ResponseCode}, ResponseDescription: {register.ResponseDescription}");
}
catch (DarajaApiException ex)
{
    Console.WriteLine($"FAILED (status {ex.StatusCode}): {ex.Message}");
}

Console.WriteLine("\n--- 6. C2B Simulate ---");
try
{
    var simulate = await client.C2B.SimulateAsync(new C2BSimulateRequest
    {
        ShortCode = shortCode,
        CommandID = nameof(C2BCommandId.CustomerPayBillOnline),
        Amount = "1",
        Msisdn = "254708374149",
        BillRefNumber = $"INV-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
    });
    Console.WriteLine($"ResponseDescription: {simulate.ResponseDescription}");
}
catch (DarajaApiException ex)
{
    Console.WriteLine($"FAILED (status {ex.StatusCode}): {ex.Message}");
}

Console.WriteLine("\n=== Done ===");
