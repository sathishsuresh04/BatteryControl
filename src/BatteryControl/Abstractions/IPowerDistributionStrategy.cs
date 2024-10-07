namespace BatteryControl.Abstractions;

public interface IPowerDistributionStrategy
{
    Task DistributePowerAsync(IList<Battery> batteries, int requestedPower);
}