using App;
using App.PreviewCreation;
using App.Settings;
using Data.Entities;
using GalleryViewer.ApiSchema;
using GalleryViewer.Middlewares;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Settings
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
builder.Services.AddTransient<AssetsService>();
builder.Services.AddTransient<FilterService>();
builder.Services.AddTransient<PreviewCreatorService>();
builder.Services.AddTransient<MediaService>();
builder.Services.AddTransient<TagsServices>();
builder.Services.AddTransient<GalleriesService>();
builder.Services.AddTransient<GroupsService>();


// API contract
// builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.SchemaFilter<NonNullablePropertiesRequiredSchemaFilter>();
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


app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("frontend");

app.UseSwagger();

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();
