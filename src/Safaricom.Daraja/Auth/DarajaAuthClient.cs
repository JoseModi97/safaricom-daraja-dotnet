using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.Exceptions;
using Safaricom.Daraja.Models;

namespace Safaricom.Daraja.Auth
{
    /// <summary>
    /// Acquires and caches OAuth 2.0 access tokens from Daraja's client-credentials endpoint
    /// (<c>GET /oauth/v1/generate?grant_type=client_credentials</c>), refreshing automatically on expiry.
    /// </summary>
    public class DarajaAuthClient
    {
        private readonly HttpClient _httpClient;
        private readonly DarajaConfig _config;
        private readonly SemaphoreSlim _tokenLock = new SemaphoreSlim(1, 1);
        private DarajaToken? _cachedToken;

        public DarajaAuthClient(DarajaConfig config, HttpClient? httpClient = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = httpClient ?? new HttpClient();
        }

        /// <summary>
        /// Returns a cached, still-valid access token, or fetches a new one from Daraja.
        /// Safe for concurrent use: only one HTTP request is issued even when many
        /// callers race to fetch a token at the same time.
        /// </summary>
        public async Task<string> GetValidAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            var current = _cachedToken;
            if (current != null && !current.IsExpired(_config.TokenExpiryBufferSeconds))
            {
                return current.AccessToken;
            }

            await _tokenLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                current = _cachedToken;
                if (current != null && !current.IsExpired(_config.TokenExpiryBufferSeconds))
                {
                    return current.AccessToken;
                }

                var token = await FetchTokenAsync(cancellationToken).ConfigureAwait(false);
                _cachedToken = token;
                return token.AccessToken;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        /// <summary>Forces the next call to <see cref="GetValidAccessTokenAsync"/> to fetch a fresh token.</summary>
        public void InvalidateCache()
        {
            _cachedToken = null;
        }

        private async Task<DarajaToken> FetchTokenAsync(CancellationToken cancellationToken)
        {
            _config.Validate();

            var url = $"{_config.GetEffectiveBaseAddress()}/oauth/v1/generate?grant_type=client_credentials";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config.ConsumerKey}:{_config.ConsumerSecret}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync(
#if NET8_0_OR_GREATER
                    cancellationToken
#endif
                ).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    throw new DarajaAuthException(
                        $"Daraja OAuth token request failed with status {(int)response.StatusCode}: {content}",
                        (int)response.StatusCode,
                        content);
                }

                var token = JsonSerializer.Deserialize<DarajaToken>(content)
                    ?? throw new DarajaAuthException("Failed to deserialize Daraja OAuth response.", (int)response.StatusCode, content);

                token.AcquiredAt = DateTimeOffset.UtcNow;
                return token;
            }
            catch (Exception ex) when (!(ex is DarajaApiException))
            {
                throw new DarajaAuthException($"Error communicating with the Daraja OAuth endpoint: {ex.Message}", innerException: ex);
            }
        }
    }
}
