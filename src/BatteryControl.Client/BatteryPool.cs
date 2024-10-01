namespace BatteryControl;

/// <summary>
/// A collection of batteries to be controlled.
/// </summary>
public class BatteryPool
{
    private readonly List<Battery> _pool = [];
    
    public BatteryPool()
    {
        for (int i = 0; i < Random.Shared.NextInt64(10, 15); i++)
        {
            _pool.Add(new Battery());
        }

        _ = RunPowerMonitor();
    }

    /// <summary>
    /// Get a list of all the connected batteries 
    /// </summary>
    /// <returns>A list of batteries</returns>
    public IList<Battery> GetConnectedBatteries()
    {
        return _pool.AsReadOnly();
    }

    private async Task RunPowerMonitor()
    {
        while (true)
        {
            await Task.Delay(1000);
            
            var currentPower = _pool.Sum(battery => battery.GetCurrentPower());
            Console.WriteLine($"Current set power: {currentPower}");
        }
    }
}