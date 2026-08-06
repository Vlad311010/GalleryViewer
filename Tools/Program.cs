/*using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddData();
builder.Services.AddApplication();

var host = builder.Build();

// var importer = host.Services.GetRequiredService<ImageImportService>();

// await importer.ImportFolder(args[0]);
*/


using App;
using App.PreviewCreation;
using App.Settings;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Tools;
using Tools.Models;

var dbOptions = new DbContextOptionsBuilder<AssetsCatalogContext>()
    .UseSqlite("Data Source=F:\\_saves\\DB\\GalleryViewerDataTest.db")
    .Options;

var configuration = new ConfigurationBuilder()
    .AddJsonFile(@"F:\_saves\.NET\GalleryViewer\Tools\appsettings.Development.json")
    .Build();

PreviewSettings? previewSettings = configuration
    .GetSection(PreviewSettings.SectionName)
    .Get<PreviewSettings>();

if (previewSettings == null)
{
    return 1;
}

IOptions<PreviewSettings> previewSettingWrapper = Options.Create(previewSettings);

using var context = new AssetsCatalogContext(dbOptions);

PreviewCreatorService previewCreatorService = new PreviewCreatorService(previewSettingWrapper);
GalleriesService galleriesService = new GalleriesService(context);
AssetsService imagesService = new AssetsService(context, previewCreatorService);
PersistenceService persistance = new PersistenceService(context);
var initializer = new GalleryInitializer(galleriesService, imagesService, persistance);

var data = new InicializationData("Test", @"F:\_saves\imgTest");

await initializer.Inicizalize(data);

return 0;

