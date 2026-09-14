using Xunit;

namespace VetCare.UnitTests;

public sealed class ProjectSetupTests
{
    [Fact]
    public void TestRunner_ShouldExecuteUnitTests()
    {
        Assert.Equal(2, 1 + 1);
    }
}