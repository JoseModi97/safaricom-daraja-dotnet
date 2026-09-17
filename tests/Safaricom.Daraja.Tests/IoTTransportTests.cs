using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Safaricom.Daraja.IoT;
using Safaricom.Daraja.IoT.Models;
using Safaricom.Daraja.IoT.Services;
using Safaricom.Daraja.IoT.Transport;
using Xunit;

namespace Safaricom.Daraja.Tests;

public class IoTTransportTests
{
    private static IoTConfig ValidConfig() => new()
    {
        ApiKey = "test-api-key",
        AccessToken = "static-test-token",
        Msisdn = "254712345678",
    };

    private static HttpResponseMessage JsonResponse(HttpStatusCode status, string body) => new(status)
    {
        Content = new StringContent(body)
    };

    [Fact]
    public async Task SendSingleMessageAsync_AttachesExpectedHeadersAndAuth()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, "{\"status\":\"ok\"}"));
        var httpClient = new HttpClient(handler);
        var client = new SimMessagingClient(new IoTTransport(ValidConfig(), httpClient));

        await client.SendSingleMessageAsync(new SendSingleMessageRequest
        {
            Msisdn = "254712345678",
            Message = "Hello",
            VpnGroup = "group1",
            Username = "operator1",
        });

        var sentRequest = Assert.Single(handler.Requests);
        Assert.Equal("Bearer", sentRequest.Headers.Authorization?.Scheme);
        Assert.Equal("static-test-token", sentRequest.Headers.Authorization?.Parameter);
        Assert.True(sentRequest.Headers.Contains("x-api-key"));
        Assert.Equal("test-api-key", sentRequest.Headers.GetValues("x-api-key").First());
        Assert.True(sentRequest.Headers.Contains("x-correlation-conversationid"));
        Assert.True(Guid.TryParse(sentRequest.Headers.GetValues("x-correlation-conversationid").First(), out _));
        Assert.Equal("web-portal", sentRequest.Headers.GetValues("X-App").First());
        Assert.Equal("254712345678", sentRequest.Headers.GetValues("X-MSISDN").First());
        Assert.Contains("/simportal/v1/sendsinglemessage", sentRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task SearchMessagesAsync_BuildsPagingQueryString()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, "{\"results\":[]}"));
        var httpClient = new HttpClient(handler);
        var client = new SimMessagingClient(new IoTTransport(ValidConfig(), httpClient));

        await client.SearchMessagesAsync(new SearchMessagesRequest
        {
            SearchValue = "test",
            VpnGroup = "group1",
            Username = "operator1",
        }, pageNo: 2, pageSize: 25);

        var sentRequest = Assert.Single(handler.Requests);
        Assert.Contains("pageNo=2", sentRequest.RequestUri!.Query);
        Assert.Contains("pageSize=25", sentRequest.RequestUri!.Query);
    }

    [Fact]
    public async Task SuspendOrUnsuspendAsync_PassesIdentityHeaderWhenProvided()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, "{}"));
        var httpClient = new HttpClient(handler);
        var client = new SimOperationsClient(new IoTTransport(ValidConfig(), httpClient));

        await client.SuspendOrUnsuspendAsync(
            new SuspendUnsuspendSubRequest { Msisdn = "254712345678", Username = "operator1", VpnGroup = "group1", Product = "data", Operation = nameof(SubscriberOperation.Suspend) },
            new IoTCallOptions { Identity = "operator1@example.com" });

        var sentRequest = Assert.Single(handler.Requests);
        Assert.Equal("operator1@example.com", sentRequest.Headers.GetValues("X-Identity").First());
    }

    [Fact]
    public async Task CheckAtiV2Async_HitsSwapCheckAtiEndpoint()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, "{}"));
        var httpClient = new HttpClient(handler);
        var client = new ImsiClient(new IoTTransport(ValidConfig(), httpClient));

        await client.CheckAtiV2Async(new CheckAtiRequest { CustomerNumber = "254712345678" });

        var sentRequest = Assert.Single(handler.Requests);
        Assert.Contains("/imsi/v2/checkATI", sentRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Validate_MissingApiKeyAndToken_Throws()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, "{}"));
        var httpClient = new HttpClient(handler);
        var client = new ImsiClient(new IoTTransport(new IoTConfig(), httpClient));

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.CheckAtiV1Async(new CheckAtiRequest { CustomerNumber = "254712345678" }));
    }

    [Fact]
    public async Task IoTTransport_WithConsumerKeyAndSecret_FetchesOAuthTokenAutomatically()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            if (request.RequestUri!.AbsolutePath.Contains("/oauth/v1/generate"))
            {
                return JsonResponse(HttpStatusCode.OK, "{\"access_token\":\"iot-oauth-token\",\"expires_in\":\"3599\"}");
            }

            Assert.Equal("iot-oauth-token", request.Headers.Authorization?.Parameter);
            return JsonResponse(HttpStatusCode.OK, "{}");
        });
        var httpClient = new HttpClient(handler);

        var config = new IoTConfig { ApiKey = "key", ConsumerKey = "ck", ConsumerSecret = "cs" };
        var client = new ImsiClient(new IoTTransport(config, httpClient));

        await client.CheckAtiV1Async(new CheckAtiRequest { CustomerNumber = "254712345678" });

        Assert.Equal(2, handler.CallCount); // one for the token, one for the actual call
    }
}
