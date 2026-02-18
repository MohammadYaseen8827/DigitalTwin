using DigitalTwinPlatform.Application.Services;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Simulations.Models;
using DigitalTwinPlatform.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace DigitalTwinPlatform.Tests.Services;

public class SyntheticDataGeneratorTests
{
    private readonly Mock<ILogger<SyntheticDataGenerator>> _mockLogger;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ISyntheticDataGeneratorRepository> _mockRepository;
    private readonly SyntheticDataGenerator _syntheticDataGenerator;

    public SyntheticDataGeneratorTests()
    {
        _mockLogger = new Mock<ILogger<SyntheticDataGenerator>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRepository = new Mock<ISyntheticDataGeneratorRepository>();
        
        _mockUnitOfWork.Setup(x => x.Repository<Domain.Entities.SyntheticDataGeneration>())
            .Returns(_mockRepository.Object);

        _syntheticDataGenerator = new SyntheticDataGenerator(
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GenerateSyntheticDataAsync_WithValidRequest_ShouldCreateData()
    {
        // Arrange
        var request = new SyntheticDataGenerationRequest
        {
            MachineType = "CNC",
            NumberOfTrajectories = 3,
            TimeRange = 30,
            RandomSeed = 12345
        };

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.SyntheticDataGeneration>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _syntheticDataGenerator.GenerateSyntheticDataAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.MachineType.Should().Be("CNC");
        result.NumberOfTrajectories.Should().Be(3);
        result.TimeRange.Should().Be(30);
        result.RandomSeed.Should().Be(12345);
        result.Status.Should().Be(Domain.Entities.GenerationStatus.Completed);
        result.DataPoints.Should().NotBeEmpty();
        result.Statistics.Should().NotBeNull();
        result.ValidationReport.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateSyntheticDataAsync_WithZeroTrajectories_ShouldStillWork()
    {
        // Arrange
        var request = new SyntheticDataGenerationRequest
        {
            MachineType = "CNC",
            NumberOfTrajectories = 0,
            TimeRange = 30,
            RandomSeed = 12345
        };

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.SyntheticDataGeneration>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _syntheticDataGenerator.GenerateSyntheticDataAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.NumberOfTrajectories.Should().Be(0);
        result.DataPoints.Should().BeEmpty();
        result.Status.Should().Be(Domain.Entities.GenerationStatus.Completed);
    }

    [Fact]
    public async Task ValidateSyntheticDataAsync_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var syntheticData = GenerateTestSyntheticData(100);
        var machineType = "CNC";

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.DataValidationReport>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _syntheticDataGenerator.ValidateSyntheticDataAsync(syntheticData, machineType);

        // Assert
        result.Should().NotBeNull();
        result.OverallScore.Should().BeGreaterThanOrEqualTo(0);
        result.OverallScore.Should().BeLessThanOrEqualTo(1);
        result.ValidationResults.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateTrajectory_WithValidParameters_ShouldCreateDataPoints()
    {
        // Arrange
        var machineType = "CNC";
        var timeRange = 30;
        var randomSeed = 12345;

        // Act
        var trajectory = _syntheticDataGenerator.GenerateTrajectory(machineType, timeRange, randomSeed);

        // Assert
        trajectory.Should().NotBeNull();
        trajectory.DataPoints.Should().NotBeEmpty();
        trajectory.DataPoints.Count.Should().BeGreaterThan(0);
        
        // Verify data points have expected structure
        var firstPoint = trajectory.DataPoints.First();
        firstPoint.Temperature.Should().BeGreaterThan(0);
        firstPoint.Vibration.Should().BeGreaterThan(0);
        firstPoint.Pressure.Should().BeGreaterThan(0);
        firstPoint.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(30));
    }

    [Fact]
    public void CalculateStatistics_WithDataPoints_ShouldReturnValidStatistics()
    {
        // Arrange
        var dataPoints = GenerateTestSyntheticData(50);

        // Act
        var statistics = _syntheticDataGenerator.CalculateStatistics(dataPoints);

        // Assert
        statistics.Should().NotBeNull();
        statistics.TotalDataPoints.Should().Be(50);
        statistics.MeanTemperature.Should().BeGreaterThan(0);
        statistics.MeanVibration.Should().BeGreaterThan(0);
        statistics.MeanPressure.Should().BeGreaterThan(0);
        statistics.MeanRpm.Should().BeGreaterThan(0);
        statistics.TemperatureStdDev.Should().BeGreaterThanOrEqualTo(0);
        statistics.VibrationStdDev.Should().BeGreaterThanOrEqualTo(0);
        statistics.PressureStdDev.Should().BeGreaterThanOrEqualTo(0);
        statistics.RpmStdDev.Should().BeGreaterThanOrEqualTo(0);
    }

    private List<Domain.Entities.SyntheticDataPoint> GenerateTestSyntheticData(int count)
    {
        var random = new Random(42); // Fixed seed for reproducible tests
        var dataPoints = new List<Domain.Entities.SyntheticDataPoint>();
        
        for (int i = 0; i < count; i++)
        {
            dataPoints.Add(new Domain.Entities.SyntheticDataPoint
            {
                Timestamp = DateTime.UtcNow.AddMinutes(-i),
                Temperature = 70 + random.NextDouble() * 30,
                Vibration = 1 + random.NextDouble() * 5,
                Pressure = 90 + random.NextDouble() * 30,
                Rpm = 1200 + random.Next(0, 800),
                HealthScore = 50 + random.NextDouble() * 50,
                Data = System.Text.Json.JsonDocument.Parse($"{{\"sensor_id\":\"TEMP-{i:D3}\",\"quality\":{random.Next(0.8, 1.0):F3}}}")
            });
        }
        
        return dataPoints;
    }
}
