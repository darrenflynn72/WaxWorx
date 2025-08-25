using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using WaxWorx.Data;
using WaxWorx.CoverArtApi;
using WaxWorx.MusicBrainzApi;
using WaxWorx.Shared.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register InventoryDbContext with a connection string
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<AdminSettingsConfig>(
    builder.Configuration.GetSection("AdminSettingsConfig"));

builder.Services.Configure<CoverArtApiConfig>(
    builder.Configuration.GetSection("CoverArtApiConfig"));

builder.Services.Configure<MusicBrainzApiConfig>(
    builder.Configuration.GetSection("MusicBrainzApiConfig"));

// services for MusicBrainz and CoverArt
builder.Services.AddHttpClient<CoverArtApiClient>();
builder.Services.AddHttpClient<MusicBrainzApiClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Album}/{action=Index}/{id?}");

app.Run();
