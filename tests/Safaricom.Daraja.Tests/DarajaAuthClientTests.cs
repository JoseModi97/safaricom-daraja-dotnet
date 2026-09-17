using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.Auth;
using Safaricom.Daraja.Exceptions;
using Safaricom.Daraja.Models;
using Xunit;

namespace Safaricom.Daraja.Tests;

/// <summary>Routes every request to a caller-supplied responder, and records how many times it was invoked.</summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
    private int _callCount;

    public int CallCount => _callCount;
    public List<HttpRequestMessage> Requests { get; } = new();

    public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _callCount);
        Requests.Add(request);
        return Task.FromResult(_responder(request));
    }
}

public class DarajaAuthClientTests
{
    private static DarajaConfig ValidConfig() => new()
    {
        ConsumerKey = "test-key",
        ConsumerSecret = "test-secret",
        Environment = DarajaEnvironment.Sandbox
    };

    private static HttpResponseMessage JsonResponse(HttpStatusCode status, string body) => new(status)
    {
        Content = new StringContent(body)
    };

    [Fact]
    public async Task GetValidAccessTokenAsync_CachesToken_AcrossCalls()
    {
        var handler = new StubHttpMessageHandler(_ =>
            JsonResponse(HttpStatusCode.OK, "{\"access_token\":\"tok-1\",\"expires_in\":\"3599\"}"));
        var httpClient = new HttpClient(handler);
        var auth = new DarajaAuthClient(ValidConfig(), httpClient);

        var first = await auth.GetValidAccessTokenAsync();
        var second = await auth.GetValidAccessTokenAsync();

        Assert.Equal("tok-1", first);
        Assert.Equal("tok-1", second);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_ConcurrentCallers_OnlyFetchOnce()
    {
        var handler = new StubHttpMessageHandler(_ =>
            JsonResponse(HttpStatusCode.OK, "{\"access_token\":\"tok-concurrent\",\"expires_in\":\"3599\"}"));
        var httpClient = new HttpClient(handler);
        var auth = new DarajaAuthClient(ValidConfig(), httpClient);

        var tasks = Enumerable.Range(0, 10).Select(_ => auth.GetValidAccessTokenAsync()).ToArray();
        var results = await Task.WhenAll(tasks);

        Assert.All(results, token => Assert.Equal("tok-concurrent", token));
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_SendsBasicAuthHeaderFromConsumerKeyAndSecret()
    {
        var handler = new StubHttpMessageHandler(_ =>
            JsonResponse(HttpStatusCode.OK, "{\"access_token\":\"tok-2\",\"expires_in\":\"3599\"}"));
        var httpClient = new HttpClient(handler);
        var auth = new DarajaAuthClient(ValidConfig(), httpClient);

        await auth.GetValidAccessTokenAsync();

        var sentRequest = Assert.Single(handler.Requests);
        Assert.Equal("Basic", sentRequest.Headers.Authorization?.Scheme);

        var expectedCredentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("test-key:test-secret"));
        Assert.Equal(expectedCredentials, sentRequest.Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_NonSuccessStatus_ThrowsDarajaAuthException()
    {
        var handler = new StubHttpMessageHandler(_ =>
            JsonResponse(HttpStatusCode.Unauthorized, "{\"error\":\"invalid_client\"}"));
        var httpClient = new HttpClient(handler);
        var auth = new DarajaAuthClient(ValidConfig(), httpClient);

        var exception = await Assert.ThrowsAsync<DarajaAuthException>(() => auth.GetValidAccessTokenAsync());
        Assert.Equal(401, exception.StatusCode);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_MissingCredentials_ThrowsBeforeSendingRequest()
    {
        var handler = new StubHttpMessageHandler(_ =>
            JsonResponse(HttpStatusCode.OK, "{\"access_token\":\"tok\",\"expires_in\":\"3599\"}"));
        var httpClient = new HttpClient(handler);
        var auth = new DarajaAuthClient(new DarajaConfig { ConsumerKey = null, ConsumerSecret = null }, httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => auth.GetValidAccessTokenAsync());
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task InvalidateCache_ForcesNextCallToRefetch()
    {
        var responses = new Queue<string>(new[]
        {
            "{\"access_token\":\"tok-a\",\"expires_in\":\"3599\"}",
            "{\"access_token\":\"tok-b\",\"expires_in\":\"3599\"}"
        });
        var handler = new StubHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, responses.Dequeue()));
        var httpClient = new HttpClient(handler);
        var auth = new DarajaAuthClient(ValidConfig(), httpClient);

        var first = await auth.GetValidAccessTokenAsync();
        auth.InvalidateCache();
        var second = await auth.GetValidAccessTokenAsync();

        Assert.Equal("tok-a", first);
        Assert.Equal("tok-b", second);
        Assert.Equal(2, handler.CallCount);
    }
}
