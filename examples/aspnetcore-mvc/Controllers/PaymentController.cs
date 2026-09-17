using Microsoft.AspNetCore.Mvc;
using Safaricom.Daraja;
using Safaricom.Daraja.AspNetCore;
using Safaricom.Daraja.Models;

namespace AspNetCoreMvcExample.Controllers;

[ApiController]
[Route("mpesa")]
public class PaymentController : ControllerBase
{
    private readonly DarajaClient _client;
    private readonly IConfiguration _configuration;

    public PaymentController(DarajaClient client, IConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
    }

    /// <summary>Raises an STK Push prompt on the customer's phone.</summary>
    [HttpPost("stk-push")]
    public async Task<IActionResult> StkPush([FromQuery] string phone, [FromQuery] decimal amount = 1)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var response = await _client.StkPush.PushAsync(new StkPushRequest
        {
            ShortCode = _configuration["Daraja:ShortCode"] ?? "174379",
            Passkey = _configuration["Daraja:Passkey"] ?? "",
            TransactionType = StkTransactionType.CustomerPayBillOnline,
            Amount = amount,
            PhoneNumber = phone,
            CallBackURL = $"{baseUrl}/mpesa/stk-callback",
            AccountReference = $"INV-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            TransactionDesc = "Example payment",
        });

        return Ok(response);
    }

    /// <summary>STK Push callback: Safaricom posts the outcome here asynchronously.</summary>
    [HttpPost("stk-callback")]
    public Task<IResult> StkCallback()
    {
        return DarajaWebhookHandler.ProcessStkCallbackAsync(Request, (callback, ctx) =>
        {
            Console.WriteLine(callback.IsSuccess
                ? $"[Daraja] STK payment confirmed: {callback.CheckoutRequestId}, Receipt: {callback.GetMetadataValue("MpesaReceiptNumber")}"
                : $"[Daraja] STK payment failed: {callback.ResultDesc}");

            return Task.CompletedTask;
        });
    }

    /// <summary>Shared Result envelope callback for B2C / B2B / Reversal / Transaction Status / Account Balance.</summary>
    [HttpPost("result")]
    public Task<IResult> Result()
    {
        return DarajaWebhookHandler.ProcessResultCallbackAsync(Request, (result, ctx) =>
        {
            Console.WriteLine(result.IsSuccess
                ? $"[Daraja] Result success: {result.TransactionId}"
                : $"[Daraja] Result failure: {result.ResultDesc}");

            return Task.CompletedTask;
        });
    }
}
