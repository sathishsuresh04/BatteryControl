using BatteryControl.Abstractions;

namespace BatteryControl;



public class BasicPowerDistributionStrategy : IPowerDistributionStrategy
{
    public async Task DistributePowerAsync(IList<Battery> batteries, int requestedPower)
{
    // Filter non-busy batteries
    var availableBatteries = batteries.Where(b => !b.IsBusy()).ToList();
    
    // Calculate the total available capacity based on the requested power direction
    var totalCapacity = availableBatteries.Sum(b => requestedPower > 0 ? b.MaxChargePower : b.MaxDischargePower);
    var remainingPower = requestedPower;

    if (totalCapacity == 0)
    {
        Console.WriteLine("Warning: No available capacity.");
        return;
    }

    // Initialize array to hold shares for each battery
    var shares = new int[availableBatteries.Count];

    // First pass: Calculate initial shares
    for (var i = 0; i < availableBatteries.Count; i++)
    {
        var battery = availableBatteries[i];

        // Skip fully charged or discharged batteries
        if ((requestedPower > 0 && battery.GetBatteryPercent() >= 100) ||
            (requestedPower < 0 && battery.GetBatteryPercent() <= 0))
        {
            continue;
        }

        // Calculate battery's share of the requested power
        var batteryCapacity = requestedPower > 0 ? battery.MaxChargePower : battery.MaxDischargePower;
        var batteryShare = (int)Math.Round((double)requestedPower * batteryCapacity / totalCapacity);

        // Ensure share does not exceed battery's capacity
        batteryShare = Math.Min(Math.Abs(batteryShare), batteryCapacity) * Math.Sign(requestedPower);

        shares[i] = batteryShare;
        remainingPower -= batteryShare;
    }

    // Second pass: Adjust for any remaining power due to rounding
    for (var i = 0; i < availableBatteries.Count && remainingPower != 0; i++)
    {
        var battery = availableBatteries[i];
        var maxAddition = (requestedPower > 0 ? battery.MaxChargePower : battery.MaxDischargePower) - Math.Abs(shares[i]);
        var additionalShare = Math.Min(Math.Abs(remainingPower), maxAddition) * Math.Sign(remainingPower);

        shares[i] += additionalShare;
        remainingPower -= additionalShare;
    }

    // Assign power shares to batteries
    for (var i = 0; i < availableBatteries.Count; i++)
    {
        if (shares[i] == 0) continue;
        try
        {
            await availableBatteries[i].SetNewPower(shares[i]);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Failed to set power for battery: {ex.Message}");
        }
    }

    if (remainingPower != 0)
    {
        Console.WriteLine($"Warning: Unable to fully distribute requested power. Remaining: {remainingPower}");
    }
}


    
}