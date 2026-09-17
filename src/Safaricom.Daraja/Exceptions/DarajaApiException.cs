using System;
using System.Text.Json;

namespace Safaricom.Daraja.Exceptions
{
    /// <summary>
    /// Thrown when a Daraja API call returns a non-success HTTP status, or a
    /// success status carrying an application-level error payload.
    /// </summary>
    public class DarajaApiException : Exception
    {
        /// <summary>The HTTP status code returned by Daraja.</summary>
        public int StatusCode { get; }

        /// <summary>Daraja's machine-readable error code (e.g. "404.001.03"), if present.</summary>
        public string? ErrorCode { get; }

        /// <summary>Daraja's request identifier for the failed call, if present.</summary>
        public string? RequestId { get; }

        /// <summary>The raw response body, for diagnostics.</summary>
        public string? RawResponse { get; }

        public DarajaApiException(string message, int statusCode, string? errorCode = null, string? requestId = null, string? rawResponse = null, Exception? innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            RequestId = requestId;
            RawResponse = rawResponse;
        }

        /// <summary>
        /// Builds an exception from a Daraja error response body, falling back to the
        /// raw body as the message when it isn't the usual {requestId, errorCode, errorMessage} shape.
        /// </summary>
        public static DarajaApiException FromResponse(int statusCode, string responseBody)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                string? errorCode = root.TryGetProperty("errorCode", out var codeEl) ? codeEl.GetString() : null;
                string? errorMessage = root.TryGetProperty("errorMessage", out var msgEl) ? msgEl.GetString() : null;
                string? requestId = root.TryGetProperty("requestId", out var reqEl) ? reqEl.GetString() : null;

                var message = !string.IsNullOrWhiteSpace(errorMessage)
                    ? errorMessage!
                    : $"Daraja API request failed with status {statusCode}.";

                return new DarajaApiException(message, statusCode, errorCode, requestId, responseBody);
            }
            catch (JsonException)
            {
                return new DarajaApiException(
                    $"Daraja API request failed with status {statusCode}: {responseBody}",
                    statusCode,
                    rawResponse: responseBody);
            }
        }
    }
}
