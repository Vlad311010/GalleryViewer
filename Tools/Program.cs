using App.Settings;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using Tools;
using Tools.Display;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

#if PUBLISHED_APP
var configPath = Path.Combine(
    AppContext.BaseDirectory,
    "appsettings.Tools.json");
#else
var configPath = Path.Combine(
    AppContext.BaseDirectory,
    "appsettings.Development.json");
#endif

var configuration = new ConfigurationBuilder()
    .AddJsonFile(configPath, optional: false, reloadOnChange: false)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

var dbOptions = new DbContextOptionsBuilder<AssetsCatalogContext>()
    .UseSqlite(configuration.GetConnectionString("GalleryViewer"))
    .Options;

PreviewSettings? previewSettings = configuration
    .GetSection(PreviewSettings.SectionName)
    .Get<PreviewSettings>();

if (previewSettings == null)
{
    return 1;
}

IOptions<PreviewSettings> previewSettingWrapper = Options.Create(previewSettings);

using var context = new AssetsCatalogContext(dbOptions);

CompositionRoot root = new(context, previewSettings);


int exitCode = await CommandProcessor.Run(args, root);

ConsoleDisplay.Display("\nPress any key to close this window. . .");
Console.ReadKey(true);
return exitCode;

