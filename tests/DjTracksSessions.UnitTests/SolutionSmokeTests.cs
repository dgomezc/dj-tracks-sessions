using Xunit;

namespace DjTracksSessions.UnitTests;

public sealed class SolutionSmokeTests
{
    [Fact]
    public void ContractsMarkerType_exists()
    {
        Assert.Equal("ApiContractMarker", typeof(DjTracksSessions.Contracts.ApiContractMarker).Name);
    }
}
