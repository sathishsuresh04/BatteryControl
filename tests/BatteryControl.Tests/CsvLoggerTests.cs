using BatteryControl.Abstractions;
using FluentAssertions;
using NSubstitute;

namespace BatteryControl.Tests;

public class CsvLoggerTests
{
   
        private readonly IBatteryPool _mockPool = Substitute.For<IBatteryPool>();
        private readonly PowerCommandSource _mockSource = new();// Substitute.For<IPowerCommandSource>();

        [Fact]
        public void CsvLogger_Constructor_ShouldCreateFileWithCorrectHeader()
        {
            // Act
            var logger = new CsvLogger(_mockPool, _mockSource);

            // Assert
            logger.FileName.Should().NotBeNullOrEmpty();
            File.Exists(logger.FileName).Should().BeTrue();
            var fileContent = File.ReadAllLines(logger.FileName);
            fileContent[0].Should().Be("Time;Target;Output");
        }
    
}