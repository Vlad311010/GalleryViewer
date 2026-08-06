using App;
using App.PreviewCreation;
using App.Settings;
using Data.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Settings
builder.Services.Configure<PreviewSettings>(
    builder.Configuration.GetSection(PreviewSettings.SectionName));

// DB
builder.Services.AddDbContext<AssetsCatalogContext>();

// Services
builder.Services.AddTransient<FilterService>();
builder.Services.AddTransient<PreviewCreatorService>();
builder.Services.AddTransient<MediaService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}



app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.Run();
