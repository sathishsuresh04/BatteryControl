using BatteryControl;

Console.WriteLine("Starting battery simulator");
var pool = new BatteryPool();
var source = new PowerCommandSource();
var logger = new CsvLogger(pool, source);
source.SetCallback(newPower =>
{
    // Beautifully expressive code goes here.
});

Console.WriteLine("Press enter to terminate");
Console.ReadLine();