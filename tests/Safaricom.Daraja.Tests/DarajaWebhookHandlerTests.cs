using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Safaricom.Daraja.AspNetCore;
using Safaricom.Daraja.Models;
using Xunit;

namespace Safaricom.Daraja.Tests;

public class DarajaWebhookHandlerTests
{
    private static HttpRequest BuildJsonRequest(string json)
    {
        var context = new DefaultHttpContext();
        var bytes = Encoding.UTF8.GetBytes(json);
        context.Request.Body = new System.IO.MemoryStream(bytes);
        context.Request.ContentType = "application/json";
        context.Request.ContentLength = bytes.Length;
        return context.Request;
    }

    [Fact]
    public async Task ProcessStkCallbackAsync_SuccessfulPayment_InvokesHandlerWithParsedMetadata()
    {
        const string json = """
        {
            "Body": {
                "stkCallback": {
                    "MerchantRequestID": "29115-34620561-1",
                    "CheckoutRequestID": "ws_CO_191220191020363925",
                    "ResultCode": 0,
                    "ResultDesc": "The service request is processed successfully.",
                    "CallbackMetadata": {
                        "Item": [
                            { "Name": "Amount", "Value": 1.00 },
                            { "Name": "MpesaReceiptNumber", "Value": "NLJ7RT61SV" },
                            { "Name": "TransactionDate", "Value": 20191219102115 },
                            { "Name": "PhoneNumber", "Value": 254708374149 }
                        ]
                    }
                }
            }
        }
        """;

        StkCallback? received = null;
        var result = await DarajaWebhookHandler.ProcessStkCallbackAsync(BuildJsonRequest(json), (callback, _) =>
        {
            received = callback;
            return Task.CompletedTask;
        });

        Assert.NotNull(received);
        Assert.True(received!.IsSuccess);
        Assert.Equal("NLJ7RT61SV", received.GetMetadataValue("MpesaReceiptNumber")?.ToString());
        Assert.IsAssignableFrom<IResult>(result);
    }

    [Fact]
    public async Task ProcessC2BValidationAsync_HandlerRejects_ReturnsRejectionPayload()
    {
        const string json = """
        {
            "TransactionType": "Pay Bill",
            "TransID": "RKTQDM7W6S",
            "TransAmount": "10",
            "BusinessShortCode": "600000",
            "BillRefNumber": "INV-001",
            "MSISDN": "254708374149"
        }
        """;

        var result = await DarajaWebhookHandler.ProcessC2BValidationAsync(BuildJsonRequest(json), (callback, _) =>
        {
            Assert.Equal("INV-001", callback.BillRefNumber);
            return Task.FromResult(C2BValidationResponse.Reject("Unknown account"));
        });

        Assert.IsAssignableFrom<IResult>(result);
    }

    [Fact]
    public async Task ProcessResultCallbackAsync_UnwrapsResultEnvelope()
    {
        const string json = """
        {
            "Result": {
                "ResultType": 0,
                "ResultCode": 0,
                "ResultDesc": "The service request is processed successfully.",
                "OriginatorConversationID": "10816-694639-1",
                "ConversationID": "AG_20191219_00004e48cf7e3533f581",
                "TransactionID": "NLJ41HAY6Q"
            }
        }
        """;

        DarajaResult? received = null;
        await DarajaWebhookHandler.ProcessResultCallbackAsync(BuildJsonRequest(json), (result, _) =>
        {
            received = result;
            return Task.CompletedTask;
        });

        Assert.NotNull(received);
        Assert.True(received!.IsSuccess);
        Assert.Equal("NLJ41HAY6Q", received.TransactionId);
    }

    [Fact]
    public async Task ProcessBillManagerReconciliationAsync_InvokesHandler()
    {
        const string json = """
        {
            "TransactionId": "RKTQDM7W6S",
            "PaidAmount": 500,
            "ExternalReference": "INV-0002",
            "phoneNumber": "254708374149"
        }
        """;

        BillManagerReconciliation? received = null;
        await DarajaWebhookHandler.ProcessBillManagerReconciliationAsync(BuildJsonRequest(json), (payload, _) =>
        {
            received = payload;
            return Task.CompletedTask;
        });

        Assert.NotNull(received);
        Assert.Equal("INV-0002", received!.ExternalReference);
        Assert.Equal(500, received.PaidAmount);
    }

    [Fact]
    public async Task ProcessRatibaCallbackAsync_HandsBackRawJsonElement()
    {
        const string json = """{ "anyField": "anyValue", "nested": { "x": 1 } }""";

        JsonElement captured = default;
        await DarajaWebhookHandler.ProcessRatibaCallbackAsync(BuildJsonRequest(json), (element, _) =>
        {
            captured = element;
            return Task.CompletedTask;
        });

        Assert.Equal("anyValue", captured.GetProperty("anyField").GetString());
    }
}
