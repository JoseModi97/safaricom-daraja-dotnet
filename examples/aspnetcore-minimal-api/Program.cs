using Safaricom.Daraja;
using Safaricom.Daraja.AspNetCore;
using Safaricom.Daraja.Models;

var builder = WebApplication.CreateBuilder(args);

// Reads the "Daraja" section from appsettings.json / environment variable overrides
// (Daraja__ConsumerKey etc.), or falls back to DARAJA_CONSUMER_KEY / DARAJA_CONSUMER_SECRET.
builder.Services.AddDaraja(builder.Configuration);

var app = builder.Build();

// This example's own extra config - not part of the SDK - your Lipa na M-Pesa Online shortcode/passkey.
var shortCode = builder.Configuration["Daraja:ShortCode"] ?? Environment.GetEnvironmentVariable("DARAJA_SHORTCODE") ?? "174379";
var passkey = builder.Configuration["Daraja:Passkey"] ?? Environment.GetEnvironmentVariable("DARAJA_PASSKEY") ?? "";

// 1. Trigger an STK Push prompt
app.MapPost("/mpesa/stk-push", async (HttpContext context, DarajaClient client, string phone, decimal? amount) =>
{
    var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";

    var response = await client.StkPush.PushAsync(new StkPushRequest
    {
        ShortCode = shortCode,
        Passkey = passkey,
        TransactionType = StkTransactionType.CustomerPayBillOnline,
        Amount = amount ?? 1,
        PhoneNumber = phone,
        CallBackURL = $"{baseUrl}/mpesa/stk-callback",
        AccountReference = $"INV-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
        TransactionDesc = "Example payment",
    });

    return Results.Ok(response);
});

// 2. STK Push callback - Safaricom posts the outcome here asynchronously
app.MapStkCallback("/mpesa/stk-callback", (callback, ctx) =>
{
    if (callback.IsSuccess)
    {
        var receipt = callback.GetMetadataValue("MpesaReceiptNumber");
        Console.WriteLine($"[Daraja] STK payment confirmed: {callback.CheckoutRequestId}, Receipt: {receipt}");
    }
    else
    {
        Console.WriteLine($"[Daraja] STK payment failed: {callback.ResultDesc}");
    }

    return Task.CompletedTask;
});

// 3. C2B Validation (accept/reject an incoming payment before it settles) and Confirmation
//    (informational - the payment has already settled by the time this fires)
// Note: when you register these with C2B.RegisterUrlAsync, Daraja rejects any
// ValidationURL/ConfirmationURL containing the word "mpesa" with a 400 - hence "/payments/..." here.
app.MapC2BValidation("/payments/c2b/validation", (callback, ctx) =>
{
    Console.WriteLine($"[Daraja] C2B validation for account {callback.BillRefNumber}");
    return Task.FromResult(C2BValidationResponse.Accept());
});

app.MapC2BConfirmation("/payments/c2b/confirmation", (callback, ctx) =>
{
    Console.WriteLine($"[Daraja] C2B confirmed: {callback.TransId}, Amount: {callback.TransAmount}");
    return Task.CompletedTask;
});

// 4. Shared Result envelope - map the same handler at both a ResultURL and QueueTimeOutURL
//    for B2C / B2B / Reversal / Transaction Status / Account Balance.
app.MapResultCallback("/mpesa/result", (result, ctx) =>
{
    Console.WriteLine(result.IsSuccess
        ? $"[Daraja] Result success: {result.TransactionId}"
        : $"[Daraja] Result failure: {result.ResultDesc}");
    return Task.CompletedTask;
});

app.Run();
