using Xunit;

namespace DjTracksSessions.IntegrationTests;

public sealed class SolutionSmokeTests
{
    [Fact]
    public void ApiProgram_can_be_referenced()
    {
        Assert.True(typeof(Program).Assembly.GetName().Name is "DjTracksSessions.Api");
    }
}
