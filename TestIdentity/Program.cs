using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TestIdentity.Areas.Identity.Data;
using TestIdentity.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("TestIdentityContextConnection") ?? throw new InvalidOperationException("Connection string 'TestIdentityContextConnection' not found.");

builder.Services.AddDbContext<TestIdentityContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDefaultIdentity<TestIdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<TestIdentityContext>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.RequireAuthorization()
.WithName("GetWeatherForecast")
.WithOpenApi();

var loginLogger = app.Services.GetRequiredService<ILogger<Login>>();

app.MapPost("/login", async (SignInManager<TestIdentityUser> signInManager, Login login) =>
{
    if (login.Email is null || login.Password is null)
    {
        return Results.BadRequest();
    }

    // This doesn't count login failures towards account lockout
    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
    var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent: true, lockoutOnFailure: false);

    if (result.Succeeded)
    {
        loginLogger.LogInformation("User logged in.");
        return Results.Ok();
    }
    if (result.RequiresTwoFactor)
    {
        throw new NotSupportedException();
    }
    if (result.IsLockedOut)
    {
        loginLogger.LogWarning("User account locked out.");
        return Results.Unauthorized();
    }
    else
    {
        loginLogger.LogInformation("Invalid login attempt.");
        return Results.Unauthorized();
    }
});

app.MapRazorPages();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public record Login(string Email, string Password);

