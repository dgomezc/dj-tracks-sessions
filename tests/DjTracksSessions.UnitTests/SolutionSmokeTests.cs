using Xunit;

namespace DjTracksSessions.UnitTests;

public sealed class SolutionSmokeTests
{
    [Fact]
    public void ContractsMarkerType_exists()
    {
        Assert.Equal("ApiContractMarker", typeof(DjTracksSessions.Contracts.ApiContractMarker).Name);
    }

    [Fact]
    public void Application_setting_requires_nonempty_key_and_value()
    {
        Assert.Throws<ArgumentException>(() => DjTrackSessions.Domain.Configuration.ApplicationSetting.Create(" ", "value"));
        Assert.Throws<ArgumentException>(() => DjTrackSessions.Domain.Configuration.ApplicationSetting.Create("key", " "));
    }
}
