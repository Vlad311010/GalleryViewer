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
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Tools;
using Tools.Models;

var options = new DbContextOptionsBuilder<AssetsCatalogContext>()
    .UseSqlite("Data Source=F:\\_saves\\DB\\GalleryViewerDataTest.db")
    .Options;

using var context = new AssetsCatalogContext(options);

GalleriesService galleriesService = new GalleriesService(context);
AssetsService imagesService = new AssetsService(context);
PersistenceService persistance = new PersistenceService(context);
var initializer = new GalleryInitializer(galleriesService, imagesService, persistance);

var data = new InicializationData("Test", @"F:\_saves\imgTest");

await initializer.Inicizalize(data);

