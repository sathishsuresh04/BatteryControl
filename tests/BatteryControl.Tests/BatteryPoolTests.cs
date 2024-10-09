using BatteryControl.Abstractions;
using FluentAssertions;
using NSubstitute;

namespace BatteryControl.Tests;

    public class BatteryPoolTests
    {
        private readonly IPowerDistributionStrategy _mockStrategy;
        private readonly IBatteryPool _batteryPool;

        public BatteryPoolTests()
        {
            _mockStrategy = Substitute.For<IPowerDistributionStrategy>();
            _batteryPool = new BatteryPool(_mockStrategy);
        }

        [Fact]
        public void GetConnectedBatteries_ShouldReturnCorrectNumberOfBatteries()
        {
            // Act
            var batteries = _batteryPool.GetConnectedBatteries();

            // Assert
            batteries.Count.Should().BeInRange(10, 15);
        }

        [Fact]
        public async Task DistributePowerAsync_ShouldCallStrategyWithCorrectParameters()
        {
            // Arrange
            const int requestedPower = 1000;

            // Act
            await _batteryPool.DistributePowerAsync(requestedPower);

            // Assert
            await _mockStrategy.Received(1).DistributePowerAsync(Arg.Any<IList<Battery>>(), requestedPower);
        }
    }
