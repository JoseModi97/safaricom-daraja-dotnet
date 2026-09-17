using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Safaricom.Daraja.Cli;

public class SetupAnswers
{
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
    public string Environment { get; set; } = "Sandbox"; // "Sandbox" or "Production"
    public string ShortCode { get; set; } = "174379";
    public string Passkey { get; set; } = string.Empty;
    public string Framework { get; set; } = "minimal"; // "minimal", "mvc", "console"
    public string TargetDir { get; set; } = string.Empty;
}

public static class ProjectScaffolder
{
    public static (string DetectedFramework, string? CsprojPath) DetectEnvironment(string targetDir)
    {
        var csprojFiles = Directory.GetFiles(targetDir, "*.csproj", SearchOption.TopDirectoryOnly);
        var csprojPath = csprojFiles.Length > 0 ? csprojFiles[0] : null;

        if (Directory.Exists(Path.Combine(targetDir, "Controllers")))
        {
            return ("mvc", csprojPath);
        }

        return ("minimal", csprojPath);
    }

    public static string UpdateAppSettings(string targetDir, SetupAnswers answers)
    {
        var settingsPath = Path.Combine(targetDir, "appsettings.Development.json");
        if (!File.Exists(settingsPath))
        {
            settingsPath = Path.Combine(targetDir, "appsettings.json");
        }

        JsonObject rootObj;
        if (File.Exists(settingsPath))
        {
            try
            {
                var text = File.ReadAllText(settingsPath);
                rootObj = JsonNode.Parse(text)?.AsObject() ?? new JsonObject();
            }
            catch
            {
                rootObj = new JsonObject();
            }
        }
        else
        {
            settingsPath = Path.Combine(targetDir, "appsettings.json");
            rootObj = new JsonObject();
        }

        rootObj["Daraja"] = new JsonObject
        {
            ["ConsumerKey"] = answers.ConsumerKey,
            ["ConsumerSecret"] = answers.ConsumerSecret,
            ["Environment"] = answers.Environment
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(settingsPath, rootObj.ToJsonString(options) + System.Environment.NewLine);

        return settingsPath;
    }

    private static string ApplyTokens(string template, SetupAnswers answers)
    {
        return template
            .Replace("__SHORTCODE__", answers.ShortCode)
            .Replace("__PASSKEY__", answers.Passkey);
    }

    public static List<string> ScaffoldFiles(string targetDir, SetupAnswers answers)
    {
        var created = new List<string>();

        switch (answers.Framework.ToLowerInvariant())
        {
            case "mvc":
            {
                var controllersDir = Path.Combine(targetDir, "Controllers");
                Directory.CreateDirectory(controllersDir);
                var controllerPath = Path.Combine(controllersDir, "DarajaController.cs");
                File.WriteAllText(controllerPath, ApplyTokens(MvcControllerTemplate, answers));
                created.Add(controllerPath);
                break;
            }

            case "console":
            {
                var demoPath = Path.Combine(targetDir, "DarajaDemo.cs");
                File.WriteAllText(demoPath, ApplyTokens(ConsoleDemoTemplate, answers));
                created.Add(demoPath);
                break;
            }

            default: // minimal
            {
                var endpointsDir = Path.Combine(targetDir, "Endpoints");
                Directory.CreateDirectory(endpointsDir);
                var endpointPath = Path.Combine(endpointsDir, "DarajaEndpoints.cs");
                File.WriteAllText(endpointPath, ApplyTokens(MinimalApiTemplate, answers));
                created.Add(endpointPath);
                break;
            }
        }

        return created;
    }

    private const string MvcControllerTemplate = @"using System;
using System.Threading.Tasks;
using Safaricom.Daraja;
using Safaricom.Daraja.AspNetCore;
using Safaricom.Daraja.Models;
using Microsoft.AspNetCore.Mvc;

namespace YourApp.Controllers;

[ApiController]
[Route(""[controller]"")]
public class DarajaController : ControllerBase
{
    private readonly DarajaClient _client;

    public DarajaController(DarajaClient client)
    {
        _client = client;
    }

    /// <summary>Raises an STK Push prompt on the customer's phone.</summary>
    [HttpPost(""stk-push"")]
    public async Task<IActionResult> StkPush([FromQuery] string phone, [FromQuery] decimal amount = 1)
    {
        var baseUrl = $""{Request.Scheme}://{Request.Host}"";

        var response = await _client.StkPush.PushAsync(new StkPushRequest
        {
            ShortCode = ""__SHORTCODE__"",
            Passkey = ""__PASSKEY__"", // from your Daraja app's Lipa na M-Pesa Online credentials
            TransactionType = StkTransactionType.CustomerPayBillOnline,
            Amount = amount,
            PhoneNumber = phone,
            CallBackURL = $""{baseUrl}/Daraja/stk-callback"",
            AccountReference = ""INV-"" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            TransactionDesc = ""Payment"",
        });

        return Ok(response);
    }

    /// <summary>STK Push callback: Safaricom posts the outcome here asynchronously.</summary>
    [HttpPost(""stk-callback"")]
    public async Task<IResult> StkCallback()
    {
        return await DarajaWebhookHandler.ProcessStkCallbackAsync(Request, async (callback, ctx) =>
        {
            if (callback.IsSuccess)
            {
                var receipt = callback.GetMetadataValue(""MpesaReceiptNumber"");
                Console.WriteLine($""[Daraja] STK payment confirmed: {callback.CheckoutRequestId}, Receipt: {receipt}"");
                // TODO: Update your database record here
            }
            else
            {
                Console.WriteLine($""[Daraja] STK payment failed: {callback.ResultDesc}"");
            }

            await Task.CompletedTask;
        });
    }
}
";

    private const string ConsoleDemoTemplate = @"using System;
using System.Threading.Tasks;
using Safaricom.Daraja;
using Safaricom.Daraja.Models;

class Program
{
    static async Task Main(string[] args)
    {
        // Reads DARAJA_CONSUMER_KEY / DARAJA_CONSUMER_SECRET / DARAJA_ENVIRONMENT env vars,
        // or pass a DarajaConfig explicitly.
        var client = new DarajaClient(new DarajaConfig());

        var response = await client.StkPush.PushAsync(new StkPushRequest
        {
            ShortCode = ""__SHORTCODE__"",
            Passkey = ""__PASSKEY__"", // from your Daraja app's Lipa na M-Pesa Online credentials
            TransactionType = StkTransactionType.CustomerPayBillOnline,
            Amount = 1,
            PhoneNumber = ""254708374149"",
            CallBackURL = ""https://example.com/mpesa/stk-callback"",
            AccountReference = ""INV-0001"",
            TransactionDesc = ""Test payment"",
        });

        Console.WriteLine($""ResponseCode: {response.ResponseCode}, CheckoutRequestID: {response.CheckoutRequestId}"");

        // Poll for the outcome (in a real app, prefer the CallBackURL instead of polling):
        await Task.Delay(TimeSpan.FromSeconds(10));

        var status = await client.StkPush.QueryAsync(new StkPushQueryRequest
        {
            ShortCode = ""__SHORTCODE__"",
            Passkey = ""__PASSKEY__"",
            CheckoutRequestId = response.CheckoutRequestId!,
        });

        Console.WriteLine($""ResultCode: {status.ResultCode}, ResultDesc: {status.ResultDesc}"");
    }
}
";

    private const string MinimalApiTemplate = @"using System;
using Safaricom.Daraja;
using Safaricom.Daraja.AspNetCore;
using Safaricom.Daraja.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace YourApp.Endpoints;

public static class DarajaEndpoints
{
    public static IEndpointRouteBuilder MapDarajaEndpoints(this IEndpointRouteBuilder app)
    {
        // 1. Trigger an STK Push prompt
        app.MapPost(""/mpesa/stk-push"", async (HttpContext context, DarajaClient client, string phone, decimal? amount) =>
        {
            var baseUrl = $""{context.Request.Scheme}://{context.Request.Host}"";

            var response = await client.StkPush.PushAsync(new StkPushRequest
            {
                ShortCode = ""__SHORTCODE__"",
                Passkey = ""__PASSKEY__"", // from your Daraja app's Lipa na M-Pesa Online credentials
                TransactionType = StkTransactionType.CustomerPayBillOnline,
                Amount = amount ?? 1,
                PhoneNumber = phone,
                CallBackURL = $""{baseUrl}/mpesa/stk-callback"",
                AccountReference = $""INV-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"",
                TransactionDesc = ""Payment"",
            });

            return Results.Ok(response);
        });

        // 2. STK Push callback: Safaricom posts the outcome here asynchronously
        app.MapStkCallback(""/mpesa/stk-callback"", async (callback, ctx) =>
        {
            if (callback.IsSuccess)
            {
                var receipt = callback.GetMetadataValue(""MpesaReceiptNumber"");
                Console.WriteLine($""[Daraja] STK payment confirmed: {callback.CheckoutRequestId}, Receipt: {receipt}"");
                // TODO: Update your database record here
            }
            else
            {
                Console.WriteLine($""[Daraja] STK payment failed: {callback.ResultDesc}"");
            }
        });

        return app;
    }
}
";
}
