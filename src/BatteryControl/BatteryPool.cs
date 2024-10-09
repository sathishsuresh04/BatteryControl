using BatteryControl.Abstractions;

namespace BatteryControl;

/// <summary>
/// A collection of batteries to be controlled.
/// </summary>

public class BatteryPool: IBatteryPool
{
    private readonly List<Battery> _pool=[];
    private readonly IPowerDistributionStrategy _distributionStrategy;

    public BatteryPool(IPowerDistributionStrategy distributionStrategy)
    {
        _distributionStrategy = distributionStrategy ?? throw new ArgumentNullException(nameof(distributionStrategy));
            for (var i = 0; i < Random.Shared.Next(10, 15); i++)
            {
                _pool.Add(new Battery());
            }
        Console.WriteLine($"Batteries: {_pool.Count}");
        _ = RunPowerMonitor();
    }
    /// <summary>
    /// Get a list of all the connected batteries 
    /// </summary>
    /// <returns>A list of batteries</returns>
    public IList<Battery> GetConnectedBatteries() => _pool.AsReadOnly();

    /// <summary>
    /// Distribute the requested power across all connected batteries asynchronously.
    /// </summary>
    /// <param name="requestedPower">The amount of power to distribute.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DistributePowerAsync(int requestedPower)
    {
        await _distributionStrategy.DistributePowerAsync(_pool, requestedPower);
    }

    private async Task RunPowerMonitor()
    {
        while (true)
        {
            await Task.Delay(1000);
            var currentPower = _pool.Sum(battery => battery.GetCurrentPower());
            Console.WriteLine($"Current set power: {currentPower}");
            
            
            // foreach (var battery in _pool)
            // {
            //     Console.WriteLine($"Battery SoC: {battery.GetBatteryPercent()}%, Current Power: {battery.GetCurrentPower()}");
            // }
        }
    }
}