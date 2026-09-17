using Safaricom.Daraja.Models;
using Xunit;

namespace Safaricom.Daraja.Tests;

public class DarajaConfigTests
{
    [Fact]
    public void GetEffectiveBaseAddress_Sandbox_ReturnsSandboxHost()
    {
        var config = new DarajaConfig { Environment = DarajaEnvironment.Sandbox };
        Assert.Equal("https://sandbox.safaricom.co.ke", config.GetEffectiveBaseAddress());
    }

    [Fact]
    public void GetEffectiveBaseAddress_Production_ReturnsProductionHost()
    {
        var config = new DarajaConfig { Environment = DarajaEnvironment.Production };
        Assert.Equal("https://api.safaricom.co.ke", config.GetEffectiveBaseAddress());
    }

    [Fact]
    public void GetEffectiveBaseAddress_ExplicitOverride_TakesPrecedenceAndTrimsTrailingSlash()
    {
        var config = new DarajaConfig
        {
            Environment = DarajaEnvironment.Production,
            BaseAddress = "https://my-mock-daraja.example.com/"
        };

        Assert.Equal("https://my-mock-daraja.example.com", config.GetEffectiveBaseAddress());
    }

    [Fact]
    public void Validate_MissingCredentials_Throws()
    {
        var config = new DarajaConfig { ConsumerKey = null, ConsumerSecret = null };
        Assert.Throws<System.InvalidOperationException>(() => config.Validate());
    }

    [Fact]
    public void Validate_WithCredentials_DoesNotThrow()
    {
        var config = new DarajaConfig { ConsumerKey = "key", ConsumerSecret = "secret" };
        config.Validate();
    }

    [Fact]
    public void DefaultEnvironment_IsSandbox()
    {
        var config = new DarajaConfig();
        Assert.Equal(DarajaEnvironment.Sandbox, config.Environment);
    }
}
