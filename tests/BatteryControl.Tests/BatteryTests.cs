using FluentAssertions;

namespace BatteryControl.Tests;


public class BatteryTests
{
    [Fact]
    public void Battery_ShouldInitializeWithRandomMaxPowers()
    {
        
        var battery = new Battery();

        battery.MaxChargePower.Should().BeInRange(100, 110);
        battery.MaxDischargePower.Should().BeInRange(100, 110);
        battery.GetBatteryPercent().Should().Be(50);
    }

    [Fact]
    public async Task SetNewPower_ShouldSetPower_WhenWithinBounds()
    {
        var battery = new Battery();
        int newPower = battery.MaxChargePower;

        await battery.SetNewPower(newPower);

        battery.GetCurrentPower().Should().Be(newPower);
    }

    [Fact]
    public async Task SetNewPower_ShouldThrow_WhenPowerExceedsLimit()
    {
        var battery = new Battery();
        int newPower = battery.MaxChargePower + 1;

        Func<Task> action = async () => await battery.SetNewPower(newPower);

        await action.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
    

    [Fact]
    public async Task SetNewPower_ShouldThrow_WhenBatteryIsBusy()
    {
        // Arrange
        var battery = new Battery();
        var setFirstPowerTask = battery.SetNewPower(10);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => battery.SetNewPower(20));

        // Cleanup
        await setFirstPowerTask;
    }
   
}