using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.Auth;
using Safaricom.Daraja.Exceptions;
using Safaricom.Daraja.Models;

namespace Safaricom.Daraja.Transport
{
    /// <summary>
    /// Shared HTTP plumbing used by every per-area service client: attaches a valid
    /// bearer token, serializes/deserializes JSON, and translates non-success
    /// responses into <see cref="DarajaApiException"/>.
    /// </summary>
    public class DarajaTransport
    {
        internal static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private readonly HttpClient _httpClient;

        public DarajaConfig Config { get; }
        public DarajaAuthClient Auth { get; }

        public DarajaTransport(DarajaConfig config, HttpClient? httpClient = null)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = httpClient ?? new HttpClient();
            Auth = new DarajaAuthClient(Config, _httpClient);
        }

        public Task<TResponse> PostAsync<TResponse>(string path, object body, CancellationToken cancellationToken = default)
        {
            return SendAsync<TResponse>(HttpMethod.Post, path, body, cancellationToken);
        }

        public Task<TResponse> GetAsync<TResponse>(string path, CancellationToken cancellationToken = default)
        {
            return SendAsync<TResponse>(HttpMethod.Get, path, null, cancellationToken);
        }

        private async Task<TResponse> SendAsync<TResponse>(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
        {
            var token = await Auth.GetValidAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            var url = $"{Config.GetEffectiveBaseAddress()}{path}";

            using var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (body != null)
            {
                var json = JsonSerializer.Serialize(body, body.GetType(), JsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

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

            if (typeof(TResponse) == typeof(string))
            {
                return (TResponse)(object)content;
            }

            return JsonSerializer.Deserialize<TResponse>(content, JsonOptions)
                ?? throw new DarajaApiException("Failed to deserialize the Daraja response.", (int)response.StatusCode, rawResponse: content);
        }
    }
}
