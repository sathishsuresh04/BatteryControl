namespace BatteryControl;

/// <summary>
/// This class sends out a stream of events with the currently requested power.
/// </summary>
public class PowerCommandSource
{
    public int Magnitude { get; private set; }

    private Action<int> _callback = _ => { };
   // public event Action<int>? PowerCommandEvent;
    private const int MaxPower = 1000;
    
    public PowerCommandSource()
    {
        _ = RunGenerator();
    }
    /// <summary>
    /// Set a callback to your code to handle the currently requested power. 
    /// </summary>
    /// <param name="callback">The callback to be called.</param>
    public void SetCallback(Action<int> callback) => _callback = callback;

    private async Task RunGenerator()
    {
        var generatorType = Random.Shared.Next(0, 2);
        var generator = generatorType switch
        {
            1 => _sineGenerator,
            _ => _squareGenerator
        };
        
        for (var second = 0; ; second++)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            try
            {
                Magnitude = generator(second);
              //  PowerCommandEvent?.Invoke(Magnitude);
                _ = Task.Run(() => _callback.Invoke(Magnitude));
                Console.WriteLine($"Current requested power: {Magnitude}");
            }
            catch (Exception ex)
            {
                 await Console.Error.WriteLineAsync("PowerCommandSource failed, it shouldn't");
                 await Console.Error.WriteLineAsync(ex.ToString());
                Environment.Exit(1);
            }
        }
    }

    private readonly Func<int, int> _sineGenerator = second =>
    {
        var angle = (double)second * 5 / 360 * 2 * Math.PI;
        var magnitude = Math.Sin(angle) * MaxPower;
        return (int)magnitude;
    };
    
    private readonly Func<int, int> _squareGenerator = second => (second % 10 < 5) ? MaxPower : -MaxPower;
}