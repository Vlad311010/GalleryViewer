using App.Interfaces.Services;
using App.Services;
using App.Settings;
using Data.Entities;
using GalleryViewer.ApiSchema;
using GalleryViewer.Middlewares;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Enums;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services);
});

// Add services to the container.
builder.Services.AddControllers();

// Settings
#if PUBLISHED_APP
builder.Configuration.AddJsonFile(
    "appsettings.App.json",
    optional: false,
    reloadOnChange: false);
#endif

builder.Services.Configure<PreviewSettings>(
    builder.Configuration.GetSection(PreviewSettings.SectionName));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// DB
builder.Services.AddDbContext<AssetsCatalogContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("GalleryViewer")));

// Services
builder.Services.AddTransient<IAssetService, AssetsService>();
builder.Services.AddTransient<IAssetsFilterService, FilterService>();
builder.Services.AddTransient<IMediaService, MediaService>();
builder.Services.AddTransient<ITagsService, TagsServices>();
builder.Services.AddTransient<IGalleriesService, GalleriesService>();
builder.Services.AddTransient<IGroupService, GroupsService>();
builder.Services.AddTransient<IMediaAccessorService, FileSystemMediaAccessorService>();


// API contract
builder.Services.AddSwaggerGen(options =>
{
    options.SchemaFilter<NonNullablePropertiesRequiredSchemaFilter>();
    options.SupportNonNullableReferenceTypes();
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSerilogRequestLogging(options =>
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set(Shared.Extensions.LoggerExtensions.AreaPropertyName, ApplicationArea.Http);
    }
);

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("frontend");

app.UseSwagger();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Lifetime.ApplicationStarted.Register(() =>
{
    foreach (var address in app.Urls)
    {
        Log.Information("GalleryViewer is running at {Address}", address);
    }
});

app.Run();
