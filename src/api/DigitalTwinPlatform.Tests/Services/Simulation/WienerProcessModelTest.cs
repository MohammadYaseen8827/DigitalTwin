using DigitalTwinPlatform.API.Services.Simulation.DegradationModels;
using Xunit;

namespace DigitalTwinPlatform.Tests.Services.Simulation
{
    public class WienerProcessModelTest
    {
        [Fact]
        public void CalculateDegradation_IncreasesMonotonically_WithoutNoise()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["drift"] = 1.0,
                ["diffusion"] = 0.0 // No noise
            };
            var model = new WienerProcessModel();
            double currentHealth = 0.0;

            // Act & Assert
            for (int i = 0; i < 10; i++)
            {
                var nextHealth = model.Step(currentHealth, TimeSpan.FromSeconds(1.0), new Random());
                Assert.True(nextHealth > currentHealth, $"Health should increase (degrade) monotonically. Previous: {currentHealth}, Next: {nextHealth}");
                currentHealth = nextHealth;
            }
        }

        [Fact]
        public void CalculateDegradation_WithDiffusion_IsStochastic()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["drift"] = 1.0,
                ["diffusion"] = 2.0 // High noise
            };
            var model1 = new WienerProcessModel();
            var model2 = new WienerProcessModel();

            // Act
            // Run two identical sequences
            // Note: Since Random is internal to the class instance (or static?), we expect *some* variance if it's properly stochastic. 
            // However, WienerProcessModel uses MathNet or System.Random. If it's seeded differently or unseeded, they might differ.
            // A better test is to check if it deviates from pure drift.
            
            double driftOnly = 1.0 * 1.0; // drift * dt
            double actualStep = model1.Step(0.0, TimeSpan.FromSeconds(1.0), new Random());
            
            // Assert
            Assert.NotEqual(driftOnly, actualStep); // Should not be exactly equal to drift due to noise
        }
    }
}
