# Safaricom.Daraja - Multi-Platform Examples

This folder contains clean, self-contained examples demonstrating how to integrate
**`Safaricom.Daraja`** and **`Safaricom.Daraja.AspNetCore`** across various .NET platforms and
architectural styles.

> **Security note**: none of the examples include real credentials. All examples use
> placeholder values (`YOUR_CONSUMER_KEY`, etc.) or read from environment variables /
> `appsettings.json`. Never commit real Consumer Key/Secret values to source control, even
> Sandbox ones.
>
> **Package references**: these examples use `ProjectReference` to the local source rather
> than `PackageReference`, since `Safaricom.Daraja` isn't published to NuGet yet. Once it is,
> swap the `ProjectReference` items in each `.csproj` for `PackageReference` to the published
> packages — everything else in these examples stays the same.

---

## Directory Index

| Platform / Template | Description | Key Features |
|---|---|---|
| [`console-script/`](./console-script) | Lightweight console application (.NET 8) | Direct `DarajaClient` usage: raise an STK Push, then query its outcome |
| [`aspnetcore-minimal-api/`](./aspnetcore-minimal-api) | ASP.NET Core Minimal API (.NET 8) | `AddDaraja()`, `MapStkCallback`/`MapC2BValidation`/`MapC2BConfirmation`/`MapResultCallback` |
| [`aspnetcore-mvc/`](./aspnetcore-mvc) | ASP.NET Core MVC Controller (.NET 8) | `PaymentController`, `DarajaWebhookHandler.ProcessStkCallbackAsync`/`ProcessResultCallbackAsync` |
| [`worker-service/`](./worker-service) | Background/hosted service (.NET 8) | Long-lived `DarajaClient` in a `BackgroundService`, recurring credential/token validation |

---

## Quick Configuration Checklist

To run any example with your own Daraja app, supply credentials via either:

### 1. `appsettings.json` (web examples)

```json
{
  "Daraja": {
    "ConsumerKey": "YOUR_CONSUMER_KEY",
    "ConsumerSecret": "YOUR_CONSUMER_SECRET",
    "Environment": "Sandbox"
  }
}
```

### 2. Environment variables (console/worker examples, or to override appsettings.json)

```bash
export DARAJA_CONSUMER_KEY="YOUR_CONSUMER_KEY"
export DARAJA_CONSUMER_SECRET="YOUR_CONSUMER_SECRET"
export DARAJA_ENVIRONMENT="Sandbox"

# Extra inputs some examples need for the Lipa na M-Pesa Online (STK Push) product specifically -
# these aren't part of the SDK's own environment variables, just this example script's inputs.
export DARAJA_SHORTCODE="174379"
export DARAJA_PASSKEY="YOUR_LIPA_NA_MPESA_PASSKEY"
```

Run any example from its folder with:

```bash
dotnet run
```
