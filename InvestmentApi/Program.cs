using InvestmentApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<InvestmentContext>(options =>
    options.UseSqlite("Data Source=investments.db"));

// Controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
                "https://osin99.github.io",   // GH Pages (origin to tylko host, ścieżka nie gra roli)
                "http://localhost:4200"       // dev
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
        );
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Investment API", Version = "v1" });
});

var app = builder.Build();

// Auto-migracja/utworzenie DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InvestmentContext>();
    db.Database.Migrate(); // lub EnsureCreated();
}

app.UseCors("AllowFrontend");

// Swagger w dev (lub ustaw zmienną SWAGGER__ENABLED=true)
if (app.Environment.IsDevelopment() ||
    builder.Configuration.GetValue<bool>("SWAGGER__ENABLED", false))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Bez HTTPS redirect w kontenerze
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

// Render port
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
