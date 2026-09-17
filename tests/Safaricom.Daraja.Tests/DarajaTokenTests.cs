using System;
using Safaricom.Daraja.Models;
using Xunit;

namespace Safaricom.Daraja.Tests;

public class DarajaTokenTests
{
    [Fact]
    public void IsExpired_FreshToken_ReturnsFalse()
    {
        var token = new DarajaToken
        {
            AccessToken = "abc",
            ExpiresIn = "3599",
            AcquiredAt = DateTimeOffset.UtcNow
        };

        Assert.False(token.IsExpired(bufferSeconds: 60));
    }

    [Fact]
    public void IsExpired_PastLifetime_ReturnsTrue()
    {
        var token = new DarajaToken
        {
            AccessToken = "abc",
            ExpiresIn = "3599",
            AcquiredAt = DateTimeOffset.UtcNow.AddSeconds(-4000)
        };

        Assert.True(token.IsExpired(bufferSeconds: 60));
    }

    [Fact]
    public void IsExpired_WithinSafetyBuffer_ReturnsTrue()
    {
        // Acquired 3550s ago with a 3599s lifetime and a 60s buffer: 3550 > (3599 - 60) = 3539, so it should already read as expired.
        var token = new DarajaToken
        {
            AccessToken = "abc",
            ExpiresIn = "3599",
            AcquiredAt = DateTimeOffset.UtcNow.AddSeconds(-3550)
        };

        Assert.True(token.IsExpired(bufferSeconds: 60));
    }

    [Fact]
    public void IsExpired_UnparsableExpiresIn_FallsBackToDefaultLifetime()
    {
        var token = new DarajaToken
        {
            AccessToken = "abc",
            ExpiresIn = "not-a-number",
            AcquiredAt = DateTimeOffset.UtcNow
        };

        Assert.False(token.IsExpired(bufferSeconds: 60));
    }
}
