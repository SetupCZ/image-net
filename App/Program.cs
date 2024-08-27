using Library;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<AppDbContext>(optionsBuilder =>
        optionsBuilder.UseSqlite("Data Source=../app.db").EnableSensitiveDataLogging(),
    ServiceLifetime.Transient);

var app = builder.Build();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
