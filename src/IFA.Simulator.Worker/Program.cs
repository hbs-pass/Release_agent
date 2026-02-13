using IFA.Simulator.Core.Pipeline;
using IFA.Simulator.Worker;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/ifa-worker-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

Log.Information("IFA Worker Service iniciando...");

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog();

builder.Services.AddIfaSimulatorCore(opts =>
{
    opts.EventIntervalMs        = 500;
    opts.EnableEmailChannel     = true;
    opts.EnableVmsChannel       = true;
    opts.EnableImChannel        = true;
    opts.EnableWebClientChannel = true;
    opts.MaxRecentEventsDisplay = 100;
});

builder.Services.AddHostedService<IfaWorker>();

var host = builder.Build();

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "IFA Worker terminó inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}