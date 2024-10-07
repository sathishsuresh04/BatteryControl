namespace BatteryControl.Abstractions;

public interface IBatteryPool
{
    IList<Battery> GetConnectedBatteries();
    Task DistributePowerAsync(int requestedPower);
}