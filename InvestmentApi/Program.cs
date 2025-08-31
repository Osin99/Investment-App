using System.IO;
using InvestmentApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- DB: ścieżka z ENV albo /data/investments.db (dla Render) ---
var dbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "/data/investments.db";
var dbDir  = Path.GetDirectoryName(dbPath);
if (!string.IsNullOrWhiteSpace(dbDir))
{
    Directory.CreateDirectory(dbDir); // upewnij się, że katalog istnieje
}
builder.Services.AddDbContext<InvestmentContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// --- Kontrolery ---
builder.Services.AddControllers();

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
                "https://osin99.github.io",  // GH Pages
                "http://localhost:4200"      // dev
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
        );
});

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Investment API", Version = "v1" });
});

var app = builder.Build();

// --- Auto-migracja/utworzenie DB ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InvestmentContext>();
    db.Database.Migrate(); // lub EnsureCreated();
}

app.UseCors("AllowFrontend");

// --- Swagger w Dev (lub przez SWAGGER__ENABLED=true) ---
if (app.Environment.IsDevelopment() ||
    builder.Configuration.GetValue<bool>("SWAGGER__ENABLED", false))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- HTTPS redirect tylko lokalnie ---
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

// --- Nasłuchiwanie na porcie Rendera ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
