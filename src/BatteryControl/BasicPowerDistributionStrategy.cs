using BatteryControl.Abstractions;

namespace BatteryControl;



public class BasicPowerDistributionStrategy : IPowerDistributionStrategy
{
public async Task DistributePowerAsync(IList<Battery> batteries, int requestedPower)
{
    if (requestedPower == 0) return;

    var availableBatteries = GetAvailableBatteries(batteries);
    if (availableBatteries.Count == 0)
    {
        Console.WriteLine("Warning: No available capacity.");
        return;
    }
    
    LogBatteryStates(availableBatteries, requestedPower);
    // Initialize array to hold shares for each battery
    var shares = new int[availableBatteries.Count];
    // First pass: Calculate initial shares
    var remainingPower = CalculateInitialShares(availableBatteries, requestedPower, shares);
    remainingPower = AdjustRemainingPower(availableBatteries, requestedPower, shares, remainingPower);

    await SetPowerToBatteries(availableBatteries, shares);

    remainingPower = await RetrySettingRemainingPower(availableBatteries, requestedPower, shares, remainingPower, 3);

    if (remainingPower != 0)
    {
        Console.WriteLine($"Final warning: Unable to fully distribute requested power. Remaining: {remainingPower}");
    }
}

/// <summary>
/// Retrieves a list of available batteries(non-busy) from the given collection.
/// </summary>
/// <param name="batteries">The collection of batteries to check for availability.</param>
/// <returns>A list of batteries that are not currently busy.</returns>
private static List<Battery> GetAvailableBatteries(IList<Battery> batteries) => batteries.Where(b => !b.IsBusy()).ToList();


private static void LogBatteryStates(IList<Battery> batteries, int requestedPower)
{
    Console.WriteLine($"Attempting to distribute {requestedPower} power to {batteries.Count} batteries.");
    for (var i = 0; i < batteries.Count; i++)
    {
        var battery = batteries[i];
        Console.WriteLine($"Battery {i}: Max capacity = {battery.MaxChargePower}/{battery.MaxDischargePower}, Current power = {battery.GetCurrentPower()}, Percent = {battery.GetBatteryPercent()}");
    }
}

private static int CalculateInitialShares(IList<Battery> batteries, int requestedPower, int[] shares)
{
    // Calculate the total available capacity based on the requested power direction
    var totalCapacity = batteries.Sum(b => requestedPower > 0 ? b.MaxChargePower : b.MaxDischargePower);
    var remainingPower = requestedPower;

    for (var i = 0; i < batteries.Count; i++)
    {
        var battery = batteries[i];
        // Skip fully charged or discharged batteries
        if ((requestedPower > 0 && battery.GetBatteryPercent() >= 100) || (requestedPower < 0 && battery.GetBatteryPercent() <= 0))
            continue;
        // Calculate battery's share of the requested power
        var batteryCapacity = requestedPower > 0 ? battery.MaxChargePower : battery.MaxDischargePower;
        var batteryShare = (int)Math.Round((double)requestedPower * batteryCapacity / totalCapacity);
        // Ensure share does not exceed battery's capacity
        batteryShare = Math.Min(Math.Abs(batteryShare), batteryCapacity) * Math.Sign(requestedPower);

        shares[i] = batteryShare;
        remainingPower -= batteryShare;
    }

    return remainingPower;
}
/// <summary>
///  Second pass: Adjust for any remaining power due to rounding
/// </summary>
/// <param name="batteries"></param>
/// <param name="requestedPower"></param>
/// <param name="shares"></param>
/// <param name="remainingPower"></param>
/// <returns></returns>
private static int AdjustRemainingPower(IList<Battery> batteries, int requestedPower, int[] shares, int remainingPower)
{
    for (var i = 0; i < batteries.Count && remainingPower != 0; i++)
    {
        var battery = batteries[i];
        var maxAddition = (requestedPower > 0 ? battery.MaxChargePower : battery.MaxDischargePower) - Math.Abs(shares[i]);
        var additionalShare = Math.Min(Math.Abs(remainingPower), maxAddition) * Math.Sign(remainingPower);

        shares[i] += additionalShare;
        remainingPower -= additionalShare;
    }

    return remainingPower;
}
/// <summary>
/// Assign power shares to batteries
/// </summary>
/// <param name="batteries"></param>
/// <param name="shares"></param>
private static async Task SetPowerToBatteries(IList<Battery> batteries, int[] shares)
{
    for (var i = 0; i < batteries.Count; i++)
    {
        if (shares[i] == 0) continue;
        try
        {
            await batteries[i].SetNewPower(shares[i]);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Failed to set power for battery: {ex.Message}");
        }
    }
}

/// <summary>
/// Attempts to reallocate any remaining power among available batteries, retrying up to a specified number of times if necessary.
/// </summary>
/// <param name="batteries">The collection of available batteries.</param>
/// <param name="requestedPower">The total power that needs to be distributed.</param>
/// <param name="shares">An array representing the current power shares of each battery.</param>
/// <param name="remainingPower">The power that has not been distributed after the initial allocation.</param>
/// <param name="retryCount">The number of retry attempts allowed for reallocating the remaining power.</param>
/// <returns>The amount of power that could not be distributed even after the retries.</returns>
private static async Task<int> RetrySettingRemainingPower(IList<Battery> batteries, int requestedPower, int[] shares,
    int remainingPower, int retryCount)
{
    while (remainingPower != 0 && retryCount-- > 0)
    {
        for (var i = 0; i < batteries.Count && remainingPower != 0; i++)
        {
            var battery = batteries[i];
            if (battery.IsBusy()) continue;

            var maxAddition = (requestedPower > 0 ? battery.MaxChargePower : battery.MaxDischargePower) - Math.Abs(shares[i]);
            var additionalShare = Math.Min(Math.Abs(remainingPower), maxAddition) * Math.Sign(remainingPower);

            if (additionalShare == 0) continue;

            try
            {
                await battery.SetNewPower(shares[i] + additionalShare);
                remainingPower -= additionalShare;
                shares[i] += additionalShare;
            }
            catch (ArgumentException)
            {
                // Handle exception if needed
            }
        }
    }

    return remainingPower;
}


    
}