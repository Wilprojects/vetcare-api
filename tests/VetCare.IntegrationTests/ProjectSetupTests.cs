using Xunit;

namespace VetCare.IntegrationTests;

public sealed class ProjectSetupTests
{
    [Fact]
    public void ApiAssembly_ShouldBeAvailable()
    {
        var apiAssembly = typeof(Program).Assembly;

        Assert.NotNull(apiAssembly);
        Assert.Equal("VetCare.Api", apiAssembly.GetName().Name);
    }
}
