using InvestmentApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Baza danych SQLite ---
builder.Services.AddDbContext<InvestmentContext>(options =>
    options.UseSqlite("Data Source=investments.db"));

// --- Kontrolery ---
builder.Services.AddControllers();

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy
            .WithOrigins(
                "http://localhost:4200",          // lokalny Angular
                "https://osin99.github.io"        // GitHub Pages
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

// --- CORS (przed MapControllers) ---
app.UseCors("AllowFrontend");

// --- Swagger w Dev (lub włącz przez zmienną środowiskową SWAGGER__ENABLED=true) ---
if (app.Environment.IsDevelopment() || 
    builder.Configuration.GetValue<bool>("SWAGGER__ENABLED", false))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- HTTPS redirect tylko lokalnie (w kontenerze nie) ---
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
