using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Safaricom.Daraja.Models;

namespace Safaricom.Daraja.AspNetCore
{
    /// <summary>
    /// Shared webhook-processing logic for Daraja callbacks, usable directly from an MVC
    /// controller action or via the Minimal API helpers in <see cref="EndpointRouteBuilderExtensions"/>.
    /// </summary>
    public static class DarajaWebhookHandler
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>Processes an STK Push <c>CallBackURL</c> notification.</summary>
        public static async Task<IResult> ProcessStkCallbackAsync(HttpRequest request, Func<StkCallback, HttpContext, Task> onReceived)
        {
            var envelope = await ReadJsonAsync<StkCallbackEnvelope>(request).ConfigureAwait(false);
            var callback = envelope?.Body?.StkCallback;

            if (callback != null)
            {
                await onReceived(callback, request.HttpContext).ConfigureAwait(false);
            }

            return Results.Ok(new { ResultCode = 0, ResultDesc = "Success" });
        }

        /// <summary>
        /// Processes a C2B Validation request. Daraja only respects the result when Validation
        /// is enabled at registration (<see cref="C2BResponseType"/> is otherwise ignored for it) —
        /// return <see cref="C2BValidationResponse.Accept"/> or <see cref="C2BValidationResponse.Reject"/> from <paramref name="validate"/>.
        /// </summary>
        public static async Task<IResult> ProcessC2BValidationAsync(HttpRequest request, Func<C2BCallback, HttpContext, Task<C2BValidationResponse>> validate)
        {
            var callback = await ReadJsonAsync<C2BCallback>(request).ConfigureAwait(false);
            var response = callback != null
                ? await validate(callback, request.HttpContext).ConfigureAwait(false)
                : C2BValidationResponse.Reject("Malformed validation payload.");

            return Results.Ok(response);
        }

        /// <summary>
        /// Processes a C2B Confirmation notification. Confirmation is informational — the
        /// payment has already settled — so this always acknowledges success once <paramref name="onConfirmed"/> runs.
        /// </summary>
        public static async Task<IResult> ProcessC2BConfirmationAsync(HttpRequest request, Func<C2BCallback, HttpContext, Task> onConfirmed)
        {
            var callback = await ReadJsonAsync<C2BCallback>(request).ConfigureAwait(false);
            if (callback != null)
            {
                await onConfirmed(callback, request.HttpContext).ConfigureAwait(false);
            }

            return Results.Ok(new { ResultCode = 0, ResultDesc = "Success" });
        }

        /// <summary>
        /// Processes the shared <c>{ "Result": { ... } }</c> envelope posted to ResultURL and
        /// QueueTimeOutURL for B2C, B2B, Reversal, Transaction Status, and Account Balance.
        /// </summary>
        public static async Task<IResult> ProcessResultCallbackAsync(HttpRequest request, Func<DarajaResult, HttpContext, Task> onResult)
        {
            var envelope = await ReadJsonAsync<DarajaResultCallback>(request).ConfigureAwait(false);
            if (envelope?.Result != null)
            {
                await onResult(envelope.Result, request.HttpContext).ConfigureAwait(false);
            }

            return Results.Ok(new { ResultCode = 0, ResultDesc = "Success" });
        }

        /// <summary>Processes a Bill Manager payment reconciliation notification posted to your opt-in callback URL.</summary>
        public static async Task<IResult> ProcessBillManagerReconciliationAsync(HttpRequest request, Func<BillManagerReconciliation, HttpContext, Task> onReconciled)
        {
            var payload = await ReadJsonAsync<BillManagerReconciliation>(request).ConfigureAwait(false);
            if (payload != null)
            {
                await onReconciled(payload, request.HttpContext).ConfigureAwait(false);
            }

            return Results.Ok(new { rescode = "200", resmsg = "Success" });
        }

        /// <summary>
        /// Processes an M-Pesa Ratiba <c>CallBackURL</c> notification as raw JSON. Ratiba's
        /// callback schema is not as consistently documented as the core transaction APIs, so
        /// this hands you the parsed <see cref="JsonElement"/> to inspect rather than a typed shape.
        /// </summary>
        public static async Task<IResult> ProcessRatibaCallbackAsync(HttpRequest request, Func<JsonElement, HttpContext, Task> onReceived)
        {
            using var document = await JsonDocument.ParseAsync(request.Body).ConfigureAwait(false);
            await onReceived(document.RootElement.Clone(), request.HttpContext).ConfigureAwait(false);

            return Results.Ok(new { ResponseCode = "0", ResponseDescription = "Success" });
        }

        private static async Task<T?> ReadJsonAsync<T>(HttpRequest request) where T : class
        {
            return await request.ReadFromJsonAsync<T>(JsonOptions).ConfigureAwait(false);
        }
    }
}
