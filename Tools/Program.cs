using App.Settings;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Tools;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
var configPath = string.Equals(environment, Environments.Development, StringComparison.OrdinalIgnoreCase)
    ? @".\appsettings.Development.json"
    : @".\appsettings.json";

var configuration = new ConfigurationBuilder()
    .AddJsonFile(configPath)
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


return await CommandProcessor.Run(args, root);

