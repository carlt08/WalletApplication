using Microsoft.EntityFrameworkCore;
using WalletApplication.API.Data;
using WalletApplication.API.Repositories;
using WalletApplication.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<WalletDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("WalletDatabase")
                      ?? "Data Source=wallet.db"));

builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IWalletService, WalletService>();

// CORS must be registered BEFORE builder.Build(); the service collection is
// read-only afterwards. Allows the Angular dev server to call the API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Create the SQLite database (with the seeded wallet) on startup if needed.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// UseCors must come before UseAuthorization and MapControllers so the CORS
// headers are applied, including to preflight OPTIONS requests.
app.UseCors("Angular");

app.UseAuthorization();

app.MapControllers();

app.Run();
