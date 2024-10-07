using System.Runtime.CompilerServices;
using BatteryControl;
using BatteryControl.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[assembly: InternalsVisibleTo("BatteryControl.Tests")]

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IPowerDistributionStrategy, BasicPowerDistributionStrategy>();
        services.AddSingleton<IBatteryPool, BatteryPool>();
        services.AddSingleton< PowerCommandSource>();
        services.AddSingleton<ICsvLogger, CsvLogger>();
    })
    .Build();

Console.WriteLine("Starting battery simulator");
var pool = host.Services.GetRequiredService<IBatteryPool>();
var source = host.Services.GetRequiredService<PowerCommandSource>();
var logger = host.Services.GetRequiredService<ICsvLogger>();

source.SetCallback(newPower =>
{
    try
    {
        //good choice for adding/expanding with mediatR command pattern here using like and the handler will call the DistributePowerAsync with the power requested
        // public class DistributePowerCommand : IRequest<bool>
        // {
        //     public int RequestedPower { get; set; }
        // }

         _ = pool.DistributePowerAsync(newPower);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error distributing power: {ex.Message}");
    }
});

Console.WriteLine("Press enter to terminate");
Console.ReadLine();