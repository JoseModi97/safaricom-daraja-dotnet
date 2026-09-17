# Safaricom.Daraja

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-netstandard2.0%20%7C%20net8.0%20%7C%20net10.0-blue.svg)](https://dotnet.microsoft.com/)
[![Sponsor via Pesapal](https://img.shields.io/badge/Sponsor_via-Pesapal-0099ff.svg?logo=heart&logoColor=white)](https://store.pesapal.com/opensourcesponsorship)

An idiomatic .NET SDK for Safaricom's **Daraja API**, with first-class Sandbox/Production
environment switching and one shared OAuth session across every product area.

Authored by [Modi97](https://www.nuget.org/profiles/Modi97) / [Jose Modi](https://github.com/JoseModi97).

> **Status: early preview, not yet published to NuGet.** All four packages below are
> implemented and tested, including live verification against the real Daraja Sandbox — see
> [Verified against the live Sandbox](#verified-against-the-live-sandbox).

---

## Coverage

| Area | Status |
|---|---|
| OAuth (client credentials) | ✅ |
| M-Pesa Express (STK Push + Query) | ✅ |
| Customer to Business (C2B register + simulate) | ✅ |
| Business to Customer (B2C, incl. B2Pochi) | ✅ |
| Business to Business (B2B, incl. Tax Remittance) | ✅ |
| Transaction Reversal | ✅ |
| Transaction Status Query | ✅ |
| Account Balance | ✅ |
| M-Pesa Ratiba (standing orders) | ✅ |
| Lipa na Bonga | ✅ (raw JSON — undocumented product, see note below) |
| Pull Transactions API | ✅ |
| Dynamic QR Code | ✅ |
| Bill Manager (opt-in, invoicing, cancellation) | ✅ |
| ASP.NET Core DI + webhook mapping | ✅ |
| CLI tool (`dotnet-safaricom-daraja`) | ✅ |
| IoT SIM-portal / IMSI (`Safaricom.Daraja.IoT`) | ✅ (raw JSON — undocumented product, see note below) |

---

## Installation

```bash
dotnet add package Safaricom.Daraja
dotnet add package Safaricom.Daraja.AspNetCore   # optional: DI + webhook mapping
dotnet add package Safaricom.Daraja.IoT          # optional: SIM-portal / IMSI (separate product, see below)
dotnet tool install --global dotnet-safaricom-daraja   # optional: CLI setup wizard
```

## Quickstart

```csharp
using Safaricom.Daraja;
using Safaricom.Daraja.Models;

var client = new DarajaClient(new DarajaConfig
{
    Environment = DarajaEnvironment.Sandbox, // or DarajaEnvironment.Production
    ConsumerKey = "YOUR_CONSUMER_KEY",
    ConsumerSecret = "YOUR_CONSUMER_SECRET",
});

// Or via environment variables: DARAJA_CONSUMER_KEY, DARAJA_CONSUMER_SECRET, DARAJA_ENVIRONMENT
// var client = new DarajaClient(new DarajaConfig());
```

Every service is a property on `DarajaClient`, sharing one cached OAuth token underneath:

### STK Push (Lipa na M-Pesa Online)

```csharp
var push = await client.StkPush.PushAsync(new StkPushRequest
{
    ShortCode = "174379",
    Passkey = "YOUR_LIPA_NA_MPESA_PASSKEY",
    TransactionType = StkTransactionType.CustomerPayBillOnline,
    Amount = 1,
    PhoneNumber = "254712345678",
    CallBackURL = "https://example.com/mpesa/stk-callback",
    AccountReference = "INV-0001",
    TransactionDesc = "Order payment",
});

var status = await client.StkPush.QueryAsync(new StkPushQueryRequest
{
    ShortCode = "174379",
    Passkey = "YOUR_LIPA_NA_MPESA_PASSKEY",
    CheckoutRequestId = push.CheckoutRequestId!,
});
```

### C2B

```csharp
await client.C2B.RegisterUrlAsync(new C2BRegisterUrlRequest
{
    ShortCode = "600000",
    ResponseType = nameof(C2BResponseType.Completed),
    // Daraja rejects ValidationURL/ConfirmationURL values containing the word "mpesa" with a
    // 400 ("Bad Request - Invalid ValidationURL - URL has the word MPESA") - confirmed live
    // against the Sandbox. Avoid it in these two URLs specifically.
    ConfirmationURL = "https://example.com/payments/c2b/confirmation",
    ValidationURL = "https://example.com/payments/c2b/validation",
});

// Sandbox only: simulate a customer paying in
await client.C2B.SimulateAsync(new C2BSimulateRequest
{
    ShortCode = "600000",
    CommandID = nameof(C2BCommandId.CustomerPayBillOnline),
    Amount = "100",
    Msisdn = "254712345678",
    BillRefNumber = "INV-0001",
});
```

### B2C / B2B / Reversal / Transaction Status / Account Balance

These all require a `SecurityCredential` — the Initiator password RSA-encrypted with
Safaricom's public certificate:

```csharp
using Safaricom.Daraja.Security;

// Download YOUR environment's certificate from the Daraja portal (Sandbox and
// Production certificates differ) — see the security note below.
var securityCredential = SecurityCredentialEncryptor.EncryptFromCertificateFile(
    "certs/production-cert.cer",
    "YourInitiatorPassword");

await client.B2C.SendAsync(new B2CRequest
{
    InitiatorName = "apiuser",
    SecurityCredential = securityCredential,
    CommandID = nameof(B2CCommandId.BusinessPayment),
    Amount = "500",
    PartyA = "600000",
    PartyB = "254712345678",
    Remarks = "Refund",
    QueueTimeOutURL = "https://example.com/mpesa/b2c/timeout",
    ResultURL = "https://example.com/mpesa/b2c/result",
});

// Tax Remittance is the same B2B endpoint under the hood:
await client.B2B.PayTaxesAsync(
    initiator: "apiuser",
    securityCredential: securityCredential,
    businessShortCode: "600000",
    kraShortCode: "YOUR_KRA_SHORTCODE_FROM_DARAJA_PORTAL",
    paymentRegistrationNumber: "PRN123456",
    amount: 1000,
    remarks: "VAT remittance",
    queueTimeOutUrl: "https://example.com/mpesa/b2b/timeout",
    resultUrl: "https://example.com/mpesa/b2b/result");
```

The `Result` (success/failure, amounts, transaction IDs) for all of the above arrives
**asynchronously** as a callback posted to your `ResultURL` — model it with
`Safaricom.Daraja.Models.DarajaResultCallback` when you build the receiving endpoint.

### M-Pesa Ratiba (standing orders)

```csharp
await client.Ratiba.CreateAsync(new RatibaCreateRequest
{
    StandingOrderName = "Monthly subscription",
    BusinessShortCode = "174379",
    TransactionType = "Standing Order Customer Pay Bill",
    Amount = "500",
    PartyA = "254712345678",
    ReceiverPartyIdentifierType = ((int)RatibaReceiverType.Paybill).ToString(),
    CallBackURL = "https://example.com/mpesa/ratiba-callback",
    AccountReference = "SUB-001",
    TransactionDesc = "Monthly subscription",
    Frequency = ((int)RatibaFrequency.Monthly).ToString(),
    StartDate = "2026-10-01",
    EndDate = "2027-10-01",
});
```

### Dynamic QR Code

```csharp
var qr = await client.QrCode.GenerateAsync(new QrCodeRequest
{
    MerchantName = "Example Store",
    ReferenceNo = "INV-0001",
    Amount = 500,
    TransactionCode = QrTransactionCode.PayBill,
    CreditPartyIdentifier = "174379",
});

System.IO.File.WriteAllBytes("qr.png", qr.ToPngBytes());
```

### Bill Manager

```csharp
await client.BillManager.OptInAsync(new BillManagerOptInRequest
{
    ShortCode = "174379",
    Email = "billing@example.com",
    OfficialContact = "254712345678",
    SendReminders = 1,
    CallbackUrl = "https://example.com/mpesa/billmanager/reconciliation",
});

await client.BillManager.SendSingleInvoiceAsync(new BillManagerInvoice
{
    ExternalReference = "INV-0002",
    BilledFullName = "Jane Doe",
    BilledPhoneNumber = "254712345678",
    BilledPeriod = "2026-09-01",
    InvoiceName = "September rent",
    DueDate = "2026-09-30",
    AccountReference = "RENT-0002",
    Amount = 15000,
    InvoiceItems = new() { new BillManagerInvoiceItem { ItemName = "Rent", Amount = 15000 } },
});
```

---

## ASP.NET Core: DI + webhook mapping

```csharp
using Safaricom.Daraja;
using Safaricom.Daraja.AspNetCore;
using Safaricom.Daraja.Models;

var builder = WebApplication.CreateBuilder(args);

// From appsettings.json section "Daraja" (ConsumerKey, ConsumerSecret, Environment, BaseAddress),
// or from DARAJA_* environment variables:
builder.Services.AddDaraja(builder.Configuration);

var app = builder.Build();

// STK Push callback
app.MapStkCallback("/mpesa/stk-callback", async (callback, ctx) =>
{
    if (callback.IsSuccess)
    {
        var receipt = callback.GetMetadataValue("MpesaReceiptNumber");
        // await orders.MarkPaidAsync(callback.CheckoutRequestId, receipt);
    }
});

// C2B: Validation (accept/reject) and Confirmation (informational, already settled)
app.MapC2BValidation("/mpesa/c2b/validation", async (callback, ctx) =>
{
    var known = await accounts.ExistsAsync(callback.BillRefNumber);
    return known ? C2BValidationResponse.Accept() : C2BValidationResponse.Reject("Unknown account");
});

app.MapC2BConfirmation("/mpesa/c2b/confirmation", async (callback, ctx) =>
{
    // await ledger.RecordAsync(callback);
});

// Shared Result envelope for B2C / B2B / Reversal / Transaction Status / Account Balance —
// map the same handler at both ResultURL and QueueTimeOutURL.
app.MapResultCallback("/mpesa/b2c/result", async (result, ctx) =>
{
    if (result.IsSuccess)
    {
        var transactionId = result.TransactionId;
        // ...
    }
});

// Bill Manager reconciliation (posted to the callback URL you gave OptInAsync)
app.MapBillManagerReconciliation("/mpesa/billmanager/reconciliation", async (payload, ctx) =>
{
    // await invoices.MarkPaidAsync(payload.ExternalReference, payload.PaidAmount);
});

app.Run();
```

Each `Map*` helper disables antiforgery on that route (these are server-to-server callbacks, not
browser form posts) and returns the acknowledgement Daraja expects automatically — your handler
only needs to react to the payload.

The same processing logic is available for MVC controllers via `DarajaWebhookHandler` (e.g.
`DarajaWebhookHandler.ProcessResultCallbackAsync(Request, onResult)` from a controller action)
if you'd rather not use Minimal API routing.

---

## CLI tool

```bash
dotnet tool install --global dotnet-safaricom-daraja
```

```bash
# Interactive setup wizard: configure credentials, pick a host style, scaffold sample code
safaricom-daraja init

# Non-interactive
safaricom-daraja init --consumer-key YOUR_KEY --consumer-secret YOUR_SECRET \
  --environment Sandbox --shortcode 174379 --passkey YOUR_PASSKEY --framework minimal -y

# Verify your credentials work against Daraja
safaricom-daraja token

# Raise an STK Push, then check its outcome
safaricom-daraja stk --shortcode 174379 --passkey YOUR_PASSKEY --phone 254712345678 \
  --amount 1 --callback-url https://example.com/mpesa/stk-callback
safaricom-daraja stk --shortcode 174379 --passkey YOUR_PASSKEY --query ws_CO_...

# Offline cryptography self-check (SecurityCredential RSA round-trip, STK password format)
safaricom-daraja test
```

`init` detects whether you're in an ASP.NET Core Minimal API, MVC, or plain console project and
scaffolds a working STK Push example (endpoint/controller + callback handler) plus an
`appsettings.json` `"Daraja"` section, wired for `AddDaraja(builder.Configuration)`.

---

## Environment switching

```csharp
var config = new DarajaConfig
{
    Environment = DarajaEnvironment.Sandbox, // default — fails safe against real money
    // Environment = DarajaEnvironment.Production,
    ConsumerKey = "...",
    ConsumerSecret = "...",
};
```

`DarajaConfig` also reads `DARAJA_CONSUMER_KEY`, `DARAJA_CONSUMER_SECRET`,
`DARAJA_ENVIRONMENT` (`Sandbox`/`Production`), and `DARAJA_BASE_ADDRESS` from environment
variables as a fallback, so credentials never need to be hardcoded.

`C2B.SimulateAsync` throws if called against `Production` — Safaricom doesn't expose a
simulate endpoint there, since live payments arrive through real M-Pesa traffic.

---

## Security note: the `SecurityCredential` certificate

B2C, B2B, Reversal, and Account Balance all require a `SecurityCredential`: your Initiator
password, RSA-encrypted with a Safaricom-issued public certificate. **This SDK does not
bundle either certificate.** Third-party copies circulating in various open-source repos
have in the past turned out to be the wrong certificate (silently producing credentials
Daraja rejects) — always download yours directly from the
[Daraja developer portal](https://developer.safaricom.co.ke) (Docs → APIs → Authorization),
and note that **Sandbox and Production use different certificates**.

```csharp
var securityCredential = SecurityCredentialEncryptor.EncryptFromCertificateFile(
    "path/to/your-downloaded-cert.cer",
    "YourInitiatorPassword");
```

---

## A known Daraja quirk this SDK handles for you

- **C2B `RegisterUrl` typo**: Daraja's own response ships the field misspelled as
  `OriginatorCoversationID`. `C2BRegisterUrlResponse.OriginatorConversationId` maps to that
  exact wire spelling so you don't have to think about it.
- **Bill Manager's inconsistent envelope**: unlike the rest of Daraja
  (`ResponseCode`/`ResponseDescription`), Bill Manager responses use `rescode`/`resmsg`.
  `BillManagerResponse` maps both.
- **OAuth `expires_in` is a string**, not a JSON number. `DarajaToken` parses it defensively.
- **C2B `RegisterUrl` rejects any callback URL containing the word "mpesa"** — confirmed live
  against the Sandbox: `ValidationURL`/`ConfirmationURL` values like
  `https://example.com/mpesa/c2b/validation` fail with a 400
  (`Bad Request - Invalid ValidationURL - URL has the word MPESA`). This SDK can't work around
  it for you since it's an outbound URL you choose — just avoid the word in that specific path;
  every example in this repo uses `/payments/...` instead.

---

## Verified against the live Sandbox

The core client, OAuth flow, and STK Push were run against Safaricom's real Sandbox during
development (not just unit-tested against mocks) using the `samples/Safaricom.Daraja.Sample.Console`
project:

- ✅ OAuth token acquisition — real tokens issued and cached correctly, across multiple runs.
- ✅ STK Push + Query — real `CheckoutRequestID`s returned and successfully queried (result
  codes varied run to run as expected: `4999` "still processing", `1037` "user cannot be
  reached" — both are Daraja telling you nobody entered a PIN on the Sandbox test number, not SDK errors).
- ✅ C2B Simulate — accepted successfully.
- ⚠️ C2B `RegisterUrl` — the request shape is confirmed correct (Daraja validated and rejected
  the URL content specifically, proving the payload parsed correctly), but registration then
  hit a `500 Service is currently unreachable` on retry. Shortcode `174379` is Safaricom's
  shared public Sandbox test shortcode used by every Daraja developer worldwide, so this looks
  like registration contention on that shortcode rather than an SDK issue — try your own
  dedicated Sandbox shortcode if you hit the same thing.
- ⚠️ Dynamic QR Code — consistently returned an HTTP 503 with an infrastructure-level "Application
  is not available" HTML page (not a Daraja JSON error), suggesting that specific Sandbox
  microservice was down during testing rather than a request-shape problem. Untested beyond that.

---

## `Safaricom.Daraja.IoT`: SIM-portal & IMSI

This is a **separate Safaricom product from core Daraja** — it shares the same Sandbox/Production
hosts, but a different auth/header scheme, and it isn't part of Safaricom's public Daraja
documentation. It's bundled here because it shipped in the same source Postman collection this
whole SDK was built from.

```csharp
using Safaricom.Daraja.IoT;
using Safaricom.Daraja.IoT.Models;

var iot = new IoTClient(new IoTConfig
{
    ApiKey = "YOUR_X_API_KEY",
    Msisdn = "254712345678", // your operating system's identifying MSISDN
    // Either supply a pre-acquired token directly...
    AccessToken = "YOUR_BEARER_TOKEN",
    // ...or let the SDK fetch/cache one via /oauth/v1/generate, same as core Daraja
    // (unconfirmed whether this product accepts that flow for your app registration - try it):
    // ConsumerKey = "...", ConsumerSecret = "...",
});

var result = await iot.Messaging.SendSingleMessageAsync(new SendSingleMessageRequest
{
    Msisdn = "254712345678",
    Message = "Your SIM has been activated.",
    VpnGroup = "your-vpn-group",
    Username = "your-portal-username",
});

Console.WriteLine(result.Json); // raw response - see note below
```

**Why raw JSON, not typed models?** Every response in this module comes back as
`DarajaRawResult.Json` rather than a typed C# object. Unlike core Daraja, this product has no
public documentation this SDK could verify response shapes against — presenting a guessed typed
model here would risk silently misleading you about field names that don't actually exist.
Request bodies, by contrast, are fully typed: they're taken directly from the reference Postman
collection's request payloads, which are unambiguous.

**What's confirmed vs. not**: the endpoints, HTTP methods, and request bodies below are taken
directly from the reference collection. The **exact semantics of the `X-MessageID` header**
(pass via `IoTCallOptions.MessageId`) are not — it resembles a conversation-continuation token
in the source collection, but this SDK cannot confirm whether it's required, generated, or
echoed back from a prior response. Leave it unset unless your integration has been told otherwise.

| Client | Covers |
|---|---|
| `iot.Messaging` | Search/filter/get-all messages, send a single message, delete a message or thread |
| `iot.SimOperations` | List SIMs, lifecycle status, customer info, activation (+ trends), rename asset, location info, suspend/unsuspend |
| `iot.Imsi` | CheckATI v1, and "SWAP CheckATI" (v2) |

---

## Lipa na Bonga: response shape is unconfirmed

Lipa na Bonga isn't part of Safaricom's public Daraja catalog, so unlike every other area in
this SDK, its response schema hasn't been cross-checked against official documentation.
`LipaNaBongaClient` returns the raw response body (`DarajaRawResult.Json`) rather than a
typed model with guessed field names. If you've confirmed the real shape against your own
sandbox, contributions adding a typed model are welcome.

---

## Examples & Samples

- [`examples/`](examples) — four self-contained, copy-pasteable demos: `console-script`,
  `aspnetcore-minimal-api`, `aspnetcore-mvc`, `worker-service`. Each currently uses a
  `ProjectReference` to the local source (not yet a `PackageReference`) since these packages
  aren't published to NuGet yet — see [`examples/README.md`](examples/README.md).
- [`samples/Safaricom.Daraja.Sample.Console`](samples/Safaricom.Daraja.Sample.Console) — part of
  the main solution; this is the project used for the live Sandbox verification above.

## Roadmap

- Publish `Safaricom.Daraja`, `Safaricom.Daraja.AspNetCore`, `Safaricom.Daraja.IoT`, and
  `dotnet-safaricom-daraja` to NuGet.org, then swap the examples' `ProjectReference`s for
  `PackageReference`s.

---

## Sponsorship

If this SDK helps you in your projects or commercial integrations, consider supporting ongoing
open-source maintenance and development:

[![Sponsor via Pesapal](https://img.shields.io/badge/Sponsor_via-Pesapal-0099ff?style=for-the-badge&logo=heart&logoColor=white)](https://store.pesapal.com/opensourcesponsorship)

👉 **[Click here to Support via Pesapal Open Source Sponsorship](https://store.pesapal.com/opensourcesponsorship)**

---

## License

MIT © [Jose Modi](https://github.com/JoseModi97) / [Modi97](https://www.nuget.org/profiles/Modi97)
