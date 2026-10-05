# safaricom-daraja-dotnet — Roadmap & Execution Plan

This document outlines the roadmap, sandbox verification strategy, and expansion tasks for `Safaricom.Daraja` (.NET SDK for Safaricom Daraja M-Pesa API).

---

## 1. Executive Summary & Goals

The `safaricom-daraja-dotnet` SDK currently provides coverage for 14+ Daraja product areas. Following the live sandbox enablement of Daraja API apps, the goal is to:
1. **Verify all endpoints live against the Sandbox** using enabled test apps.
2. **Harden the HTTP transport** against Incapsula WAF blocks (explicit `User-Agent` header).
3. **Expand example applications (`examples/`)** beyond STK Push to cover Dynamic QR Code, B2C disbursement, C2B simulation, and Transaction Status reconciliation.
4. **Publish `v0.3.0`** to NuGet with verified multi-platform samples and integration test harnesses.

---

## 2. API Coverage & Sandbox Status

| Area / Client | Sandbox Status | Implementation Plan & Tasks |
|---|---|---|
| **OAuth (`client.Auth`)** | ✅ Verified | Already fully tested and passing. |
| **Dynamic QR (`client.QrCode`)** | ✅ Verified in Sandbox | Wire value `"PB"` / `"BG"` returns Base64 PNG. Add QR endpoint to `examples/aspnetcore-minimal-api`. |
| **STK Push & Query (`client.StkPush`)** | ✅ Verified | Already implemented in all 4 example projects. Add resilience against spike arrest (HTTP 429). |
| **C2B (`client.C2B`)** | ✅ Verified | Register URLs (v1/v2) and Simulation passing with `600980`. Add simulation console task to `console-script/`. |
| **B2C (`client.B2C`)** | ⚠️ Needs WAF Header | Endpoint returns `ResponseCode: 0` when `User-Agent` is specified. Requires `DarajaTransport` header update. |
| **B2B / Tax Remittance (`client.B2B`)** | ⚠️ Needs WAF Header | Test with sandbox shortcodes `600981` -> `600000`. |
| **Account Balance (`client.AccountBalance`)** | ⚠️ Asynchronous Result | Initiates successfully with `testapi`; callback requires public tunnel (ngrok) on `ResultURL`. |
| **Transaction Status (`client.TransactionStatus`)** | ⚠️ Asynchronous Result | Initiates successfully with `testapi`; callback requires public tunnel (ngrok) on `ResultURL`. |
| **Transaction Reversal (`client.Reversal`)** | ⚠️ Needs valid TrxID | Requires real settled transaction ID to test in sandbox. |
| **Pull Transactions (`client.PullApi`)** | ℹ️ Portal Dependent | Requires Nominated Number configured on Daraja portal. |
| **M-Pesa Ratiba (`client.Ratiba`)** | ℹ️ Portal Dependent | Requires specific Ratiba shortcode issued for the application. |
| **Bill Manager (`client.BillManager`)** | ℹ️ Onboarding Dependent | Opt-in and invoice generation testing in sandbox. |

---

## 3. Step-by-Step Action Items

### Phase 1: Transport Hardening & WAF Protection
- **Target**: `src/Safaricom.Daraja/Transport/DarajaTransport.cs`
- **Change**: Add default `User-Agent` header (`Safaricom.Daraja/<version> (.NET)`).
- **Rationale**: Safaricom's Incapsula WAF blocks generic HTTP clients without custom user agents on B2C, B2B, and Reversal endpoints with HTTP 403.

### Phase 2: Expand `examples/`
Currently, the examples focus almost exclusively on STK Push:
- **`examples/aspnetcore-minimal-api`**:
  - Add `app.MapGet("/qr", ...)` endpoint to generate and return a dynamic QR code PNG stream directly (`Results.File(qr.ToPngBytes(), "image/png")`).
  - Add C2B simulate route for local sandbox testing.
- **`examples/console-script`**:
  - Add menu or CLI flags to test QR Code generation and save PNG to disk.
  - Add B2C payment test flow with test recipient.
- **`examples/aspnetcore-mvc`**:
  - Add a QR payment display view in `PaymentController`.

### Phase 3: Integration Test Runner (`samples/Safaricom.Daraja.Sample.Console`)
- Enhance `samples/Safaricom.Daraja.Sample.Console/Program.cs` to run end-to-end sandbox tests across:
  1. OAuth token retrieval.
  2. Dynamic QR Code generation.
  3. STK Push & Query.
  4. C2B URL registration and simulation.
  5. B2C payment request.
  6. Account Balance query.
  7. Transaction Status query.

### Phase 4: Version Bump & Release `v0.3.0`
1. Update `Directory.Build.props` / `.csproj` versions to `0.3.0`.
2. Update `README.md` and `CHANGELOG.md`.
3. Run `dotnet test -c Release`.
4. Commit, tag `v0.3.0`, and trigger GitHub Actions release workflow.

---

## 4. Environment Variables Checklist for Sandbox Testing

```bash
export DARAJA_CONSUMER_KEY="<YOUR_CONSUMER_KEY>"
export DARAJA_CONSUMER_SECRET="<YOUR_CONSUMER_SECRET>"
export DARAJA_ENVIRONMENT="Sandbox"
export DARAJA_SHORTCODE="174379"
export DARAJA_PASSKEY="<YOUR_PASSKEY>"
export DARAJA_INITIATOR_NAME="testapi"
export DARAJA_INITIATOR_PASSWORD="<TEST_PASSWORD>"
```
