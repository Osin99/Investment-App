using InvestmentApi.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Dodaj serwis bazy danych SQLite
builder.Services.AddDbContext<InvestmentContext>(options =>
    options.UseSqlite("Data Source=investments.db"));

// Dodaj kontrolery
builder.Services.AddControllers();

// Dodajemy CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Investment API", Version = "v1" });
});

var app = builder.Build();

// Używamy CORS (musi być przed MapControllers)
app.UseCors("AllowFrontend");

// Środowisko deweloperskie – uruchom Swaggera
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();  // <-- To uruchamia kontrolery API

app.Run();
