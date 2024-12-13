using CacheSimulatorWebApp.Models;
using CacheSimulatorWebApp.Models.Policy.Mapping;
using CacheSimulatorWebApp.Models.Policy.Write;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddLogging();
builder.Services.AddControllersWithViews();

// builder.Services.AddScoped<Memory>();
// builder.Services.AddScoped<CacheLine>();
// builder.Services.AddScoped<IMappingPolicy, DirectMapping>();
// builder.Services.AddScoped<IWritePolicy, WriteBack>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
