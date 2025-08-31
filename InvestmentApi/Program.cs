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
        policy.WithOrigins(
                "http://localhost:4200",                // lokalny Angular
                "https://osin99.github.io")             // GitHub Pages
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

// --- Użycie CORS (przed MapControllers) ---
app.UseCors("AllowFrontend");

// --- Swagger tylko w Dev ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
