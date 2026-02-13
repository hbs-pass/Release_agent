using IFA.Simulator.Core.Pipeline;
using IFA.Simulator.Web.Services;
using MudBlazor.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/ifa-web-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddIfaSimulatorCore(opts =>
{
    opts.EventIntervalMs        = 2000;
    opts.EnableEmailChannel     = true;
    opts.EnableVmsChannel       = true;
    opts.EnableImChannel        = true;
    opts.EnableWebClientChannel = true;
    opts.MaxRecentEventsDisplay = 50;
});

builder.Services.AddSingleton<DashboardService>();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapHealthChecks("/health");

app.MapRazorComponents<IFA.Simulator.Web.Components.App>()
    .AddInteractiveServerRenderMode();

var dashboard = app.Services.GetRequiredService<DashboardService>();
_ = dashboard.StartAsync();

await app.RunAsync();