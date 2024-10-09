namespace BatteryControl;

public class Battery
{
    private int _currentPower;
    private bool _isBusy;
    private double _batteryPercent = 50;
    
    public virtual int MaxChargePower { get; }
    public virtual int MaxDischargePower { get; }


    public Battery()
    {
        MaxChargePower = Random.Shared.Next(100, 110);
        MaxDischargePower = Random.Shared.Next(100, 110);

        _ = UpdateSoC();
    }
    

    /// <summary>
    /// Sets the battery to charge (positive values) or discharge (negative values).
    /// </summary>
    /// <param name="newPower">How many watts the battery should charge/discharge</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public  virtual async Task SetNewPower(int newPower)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(newPower, MaxChargePower);

        ArgumentOutOfRangeException.ThrowIfLessThan(newPower, -MaxDischargePower);

        if (_isBusy)
        {
            throw new ArgumentException("Battery is busy");
        }

        _isBusy = true;
        await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(500, 2500)));
        _currentPower = newPower;
        _isBusy = false;
    }
    
    /// <summary>
    /// Returns the current amount of watts the battery charges/dischages.
    /// </summary>
    /// <returns></returns>
    public virtual int GetCurrentPower()
    {
        switch (_currentPower)
        {
            case > 0 when _batteryPercent >= 100:
            case < 0 when _batteryPercent <= 0:
                return 0;
            default:
                return _currentPower;
        }
    }

    //You may uncomment this line if you need it.
    public  virtual bool IsBusy() => _isBusy;

    /// <summary>
    /// Returns how much the battery is charged. If it is 0% the battery is empty, and 100% it's full.
    /// If the battery is empty, it cannot discharge more. And if it's 100% it cannot charge more.
    /// </summary>
    /// <returns>The percentage of charge the battery can have.</returns>
    public  virtual int GetBatteryPercent()
    {
        return (int)_batteryPercent;
    }

    private async Task UpdateSoC()
    {
        while (true)
        {
            switch (_currentPower)
            {
                case > 0 when _batteryPercent < 100:
                    //charge is slower due to energy losses in the form of heat and Charging rates are often limited to prevent overheating and extend battery life.
                    //Charge is not symetric with discharge
                    _batteryPercent += (double)_currentPower / 1200;
                    break;
                case < 0 when _batteryPercent > 0:
                    //discharging is faster
                    _batteryPercent += (double)_currentPower / 1000;
                    break;
            }

            await Task.Delay(1000);
        }
    }
}