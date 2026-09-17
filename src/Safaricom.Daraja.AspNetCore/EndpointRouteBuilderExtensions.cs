using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Safaricom.Daraja.Models;

namespace Safaricom.Daraja.AspNetCore
{
    /// <summary>Minimal API extensions for mapping Daraja webhook/callback endpoints.</summary>
    public static class EndpointRouteBuilderExtensions
    {
        /// <summary>Maps an endpoint that receives STK Push callbacks at the URL you passed as <c>CallBackURL</c>.</summary>
        public static RouteHandlerBuilder MapStkCallback(this IEndpointRouteBuilder endpoints, string pattern, Func<StkCallback, HttpContext, Task> onReceived)
        {
            // Explicitly typed as Func<HttpContext, Task<IResult>> (rather than passing the lambda
            // inline) so overload resolution picks MapPost(string, Delegate) — the Minimal API
            // overload returning RouteHandlerBuilder — instead of silently binding to the older
            // MapPost(string, RequestDelegate) overload, which Task<IResult> is also convertible to.
            Func<HttpContext, Task<IResult>> handler = context => DarajaWebhookHandler.ProcessStkCallbackAsync(context.Request, onReceived);
            var route = endpoints.MapPost(pattern, handler);
            route.DisableAntiforgery();
            return route;
        }

        /// <summary>Maps an endpoint that receives C2B Validation requests at the URL registered as <c>ValidationURL</c>.</summary>
        public static RouteHandlerBuilder MapC2BValidation(this IEndpointRouteBuilder endpoints, string pattern, Func<C2BCallback, HttpContext, Task<C2BValidationResponse>> validate)
        {
            Func<HttpContext, Task<IResult>> handler = context => DarajaWebhookHandler.ProcessC2BValidationAsync(context.Request, validate);
            var route = endpoints.MapPost(pattern, handler);
            route.DisableAntiforgery();
            return route;
        }

        /// <summary>Maps an endpoint that receives C2B Confirmation notifications at the URL registered as <c>ConfirmationURL</c>.</summary>
        public static RouteHandlerBuilder MapC2BConfirmation(this IEndpointRouteBuilder endpoints, string pattern, Func<C2BCallback, HttpContext, Task> onConfirmed)
        {
            Func<HttpContext, Task<IResult>> handler = context => DarajaWebhookHandler.ProcessC2BConfirmationAsync(context.Request, onConfirmed);
            var route = endpoints.MapPost(pattern, handler);
            route.DisableAntiforgery();
            return route;
        }

        /// <summary>
        /// Maps an endpoint that receives the shared Result callback used by B2C, B2B,
        /// Reversal, Transaction Status, and Account Balance. Map the same handler at both
        /// your ResultURL and QueueTimeOutURL — the envelope shape is identical; <see cref="DarajaResult.ResultCode"/>
        /// tells you which case you're in.
        /// </summary>
        public static RouteHandlerBuilder MapResultCallback(this IEndpointRouteBuilder endpoints, string pattern, Func<DarajaResult, HttpContext, Task> onResult)
        {
            Func<HttpContext, Task<IResult>> handler = context => DarajaWebhookHandler.ProcessResultCallbackAsync(context.Request, onResult);
            var route = endpoints.MapPost(pattern, handler);
            route.DisableAntiforgery();
            return route;
        }

        /// <summary>Maps an endpoint that receives Bill Manager payment reconciliation notifications at your opt-in callback URL.</summary>
        public static RouteHandlerBuilder MapBillManagerReconciliation(this IEndpointRouteBuilder endpoints, string pattern, Func<BillManagerReconciliation, HttpContext, Task> onReconciled)
        {
            Func<HttpContext, Task<IResult>> handler = context => DarajaWebhookHandler.ProcessBillManagerReconciliationAsync(context.Request, onReconciled);
            var route = endpoints.MapPost(pattern, handler);
            route.DisableAntiforgery();
            return route;
        }

        /// <summary>
        /// Maps an endpoint that receives M-Pesa Ratiba callbacks at the URL you passed as
        /// <c>CallBackURL</c>, as raw JSON (see <see cref="DarajaWebhookHandler.ProcessRatibaCallbackAsync"/> for why).
        /// </summary>
        public static RouteHandlerBuilder MapRatibaCallback(this IEndpointRouteBuilder endpoints, string pattern, Func<JsonElement, HttpContext, Task> onReceived)
        {
            Func<HttpContext, Task<IResult>> handler = context => DarajaWebhookHandler.ProcessRatibaCallbackAsync(context.Request, onReceived);
            var route = endpoints.MapPost(pattern, handler);
            route.DisableAntiforgery();
            return route;
        }
    }
}
