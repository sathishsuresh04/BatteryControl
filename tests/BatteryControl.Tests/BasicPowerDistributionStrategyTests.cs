using NSubstitute;

namespace BatteryControl.Tests;


public class BasicPowerDistributionStrategyTests
{
    private readonly BasicPowerDistributionStrategy _strategy = new();
    
    [Fact]
    public async Task DistributePowerAsync_ShouldDistributePowerProportionally()
    {
        // Arrange
        var battery1 = Substitute.For<Battery>();
        var battery2 = Substitute.For<Battery>();
        SetupBattery(battery1, 50, 75, 75);
        SetupBattery(battery2, 50, 25, 25);
        var batteries = new List<Battery> { battery1, battery2 };

        // Act
        await _strategy.DistributePowerAsync(batteries, 100);

        // Assert
        await battery1.Received(1).SetNewPower(75);
        await battery2.Received(1).SetNewPower(25);
    }
    
    [Fact]
    public async Task DistributePowerAsync_ShouldHandleBusyBatteries()
    {
        // Arrange
        var battery1 = Substitute.For<Battery>();
        var battery2 = Substitute.For<Battery>();
        SetupBattery(battery1, 50, 100, 100, true);
        SetupBattery(battery2, 50, 100, 100);
        var batteries = new List<Battery> { battery1, battery2 };

        // Act
        await _strategy.DistributePowerAsync(batteries, 100);

        // Assert
        await battery1.DidNotReceive().SetNewPower(Arg.Any<int>());
        await battery2.Received(1).SetNewPower(100);
    }

    private static void SetupBattery(Battery battery, int batteryPercent, int maxChargePower, int maxDischargePower, bool isBusy = false)
    {
        battery.GetBatteryPercent().Returns(batteryPercent);
        battery.MaxChargePower.Returns(maxChargePower);
        battery.MaxDischargePower.Returns(maxDischargePower);
        battery.IsBusy().Returns(isBusy);
    }
    [Fact]
    public async Task DistributePowerAsync_ShouldNotExceedMaxCapacity()
    {
        // Arrange
        var battery = Substitute.For<Battery>();
        SetupBattery(battery, 50, 100, 100);  // 50% battery; 100 max capacities
        var batteries = new List<Battery> { battery };

        // Act
        await _strategy.DistributePowerAsync(batteries, 100);

        // Assert
        await battery.Received(1).SetNewPower(100);  // No over-allocation beyond max capacity
    }
}