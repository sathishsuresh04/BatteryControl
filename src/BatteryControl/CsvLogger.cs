using System.Diagnostics;
using BatteryControl.Abstractions;

namespace BatteryControl;

/// <summary>
/// This class monitors the current requested power, and the current output power.
/// It writes the values every second to a temp file you can read later. 
/// </summary>
internal class CsvLogger: ICsvLogger
{
    private readonly IBatteryPool _pool;
    private readonly PowerCommandSource _source;

    public CsvLogger(IBatteryPool pool, PowerCommandSource source)
    {
        _pool = pool;
        _source = source;
        FileName = Path.GetTempFileName() + ".csv";
        Console.WriteLine($"Logging to {FileName}");
        const string logLine = "Time;Target;Output";
        File.AppendAllLines(FileName, [logLine]);

        _ = Task.Run(StartLogger);
    }

    private async void StartLogger()
    {
        var sw = Stopwatch.StartNew();
        while (true)
        {
            var actualOutput = _pool.GetConnectedBatteries().Sum(battery => battery.GetCurrentPower());
            var requestedPower = _source.Magnitude;
            var logLine = $"{sw.Elapsed};{requestedPower};{actualOutput}";
            await File.AppendAllLinesAsync(FileName, [logLine]);
            await Task.Delay(100);
        }
    }

    public string FileName { get; }
}