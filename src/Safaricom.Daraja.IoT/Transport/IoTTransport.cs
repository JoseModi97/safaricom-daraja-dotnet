using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.Auth;
using Safaricom.Daraja.Exceptions;
using Safaricom.Daraja.Models;

namespace Safaricom.Daraja.IoT.Transport
{
    /// <summary>
    /// Optional per-call header overrides. <see cref="MessageId"/> and <see cref="Identity"/>
    /// are opaque, undocumented values observed in the reference collection (the former looks
    /// like a conversation-continuation token, the latter an operator email) — pass through
    /// whatever your integration has been given for them if you have it.
    /// </summary>
    public class IoTCallOptions
    {
        public string? MessageId { get; set; }
        public string? Identity { get; set; }
    }

    /// <summary>Shared HTTP plumbing for the IoT SIM-portal / IMSI endpoints.</summary>
    public class IoTTransport
    {
        private readonly HttpClient _httpClient;
        private readonly DarajaAuthClient? _authClient;

        public IoTConfig Config { get; }

        public IoTTransport(IoTConfig config, HttpClient? httpClient = null)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = httpClient ?? new HttpClient();

            if (string.IsNullOrWhiteSpace(config.AccessToken) &&
                !string.IsNullOrWhiteSpace(config.ConsumerKey) &&
                !string.IsNullOrWhiteSpace(config.ConsumerSecret))
            {
                var darajaConfig = new DarajaConfig
                {
                    Environment = config.Environment,
                    BaseAddress = config.BaseAddress,
                    ConsumerKey = config.ConsumerKey,
                    ConsumerSecret = config.ConsumerSecret
                };
                _authClient = new DarajaAuthClient(darajaConfig, _httpClient);
            }
        }

        public Task<string> PostAsync(string path, object body, IoTCallOptions? options = null, CancellationToken cancellationToken = default)
        {
            return PostAsync(path, queryParams: null, body, options, cancellationToken);
        }

        public async Task<string> PostAsync(string path, IReadOnlyDictionary<string, string>? queryParams, object body, IoTCallOptions? options = null, CancellationToken cancellationToken = default)
        {
            Config.Validate();

            var url = BuildUrl(path, queryParams);
            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            var token = await ResolveAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            ApplyCommonHeaders(request, options);

            var json = JsonSerializer.Serialize(body, body.GetType());
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(
#if NET8_0_OR_GREATER
                cancellationToken
#endif
            ).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw DarajaApiException.FromResponse((int)response.StatusCode, content);
            }

            return content;
        }

        private async Task<string> ResolveAccessTokenAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(Config.AccessToken))
            {
                return Config.AccessToken!;
            }

            if (_authClient != null)
            {
                return await _authClient.GetValidAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            }

            throw new InvalidOperationException("No access token available. Set IoTConfig.AccessToken, or both ConsumerKey and ConsumerSecret.");
        }

        private void ApplyCommonHeaders(HttpRequestMessage request, IoTCallOptions? options)
        {
            request.Headers.TryAddWithoutValidation("x-correlation-conversationid", Guid.NewGuid().ToString());
            request.Headers.TryAddWithoutValidation("x-source-system", Config.SourceSystem);
            request.Headers.TryAddWithoutValidation("x-api-key", Config.ApiKey);
            request.Headers.TryAddWithoutValidation("X-App", Config.App);

            if (!string.IsNullOrWhiteSpace(Config.Msisdn))
            {
                request.Headers.TryAddWithoutValidation("X-MSISDN", Config.Msisdn);
            }

            if (!string.IsNullOrWhiteSpace(Config.AcceptLanguage))
            {
                request.Headers.TryAddWithoutValidation("Accept-Language", Config.AcceptLanguage);
            }

            var messageId = options?.MessageId;
            if (!string.IsNullOrWhiteSpace(messageId))
            {
                request.Headers.TryAddWithoutValidation("X-MessageID", messageId);
            }

            var identity = options?.Identity;
            if (!string.IsNullOrWhiteSpace(identity))
            {
                request.Headers.TryAddWithoutValidation("X-Identity", identity);
            }
        }

        private string BuildUrl(string path, IReadOnlyDictionary<string, string>? queryParams)
        {
            var url = $"{Config.GetEffectiveBaseAddress()}{path}";
            if (queryParams == null || queryParams.Count == 0)
            {
                return url;
            }

            var query = new StringBuilder();
            foreach (var kvp in queryParams)
            {
                query.Append(query.Length == 0 ? '?' : '&');
                query.Append(Uri.EscapeDataString(kvp.Key));
                query.Append('=');
                query.Append(Uri.EscapeDataString(kvp.Value));
            }

            return url + query;
        }
    }
}
