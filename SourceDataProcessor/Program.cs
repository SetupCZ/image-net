// See https://aka.ms/new-console-template for more information

using Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SourceDataProcessor;

var builder = Host.CreateApplicationBuilder(args);

var rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));
var dbPath = Path.Combine(rootPath, "app.db");

builder.Services.AddDbContext<AppDbContext>(optionsBuilder =>
        optionsBuilder.UseSqlite($"Data Source={dbPath}",
            contextOptionsBuilder => contextOptionsBuilder.MigrationsAssembly("Library")),
    ServiceLifetime.Transient);

builder.Services.AddTransient<TreeReader>();

var app = builder.Build();

using var scope = app.Services.CreateScope();

scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
var streamReader = scope.ServiceProvider.GetRequiredService<TreeReader>();

await using var stream = File.OpenRead("structure_released.xml");

await streamReader.Read(stream);

await app.StopAsync();
