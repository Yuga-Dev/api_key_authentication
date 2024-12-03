using Api_Key_Authentication;
using Api_Key_Authentication.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("AuthDemoDb"));

builder.Services.AddAuthentication("ApiKeyScheme").AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKeyScheme", options => { });

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "Required API Key",
        In = ParameterLocation.Header,
        Name = "X-API-KEY",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();


app.MapPost("/auth/register", async (User user, AppDbContext db) =>
{
    var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
    if (existingUser != null)
    {
        return Results.BadRequest("User already exists.");
    }

    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
    user.Password = null;
    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok("User registered successfully.");
});

app.MapPost("/auth/request-key", async (UserLogin login, AppDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == login.Email);
    if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var apiKey = Guid.NewGuid().ToString();
    var hashedKey = BCrypt.Net.BCrypt.HashPassword(apiKey);

    db.ApiKeys.Add(new ApiKey
    {
        UserId = user.Id,
        KeyHash = hashedKey,
        CreatedAt = DateTime.UtcNow,
        ExpiryDate = DateTime.UtcNow.AddMonths(6),
        IsActive = true
    });

    await db.SaveChangesAsync();

    return Results.Ok(new { ApiKey = apiKey });
});



app.MapPost("/auth/revoke-key", async (string apiKey, AppDbContext db) =>
{
    var apiKeys = await db.ApiKeys.Where(k => k.IsActive).ToListAsync();

    var key = apiKeys.FirstOrDefault(k => BCrypt.Net.BCrypt.Verify(apiKey, k.KeyHash));

    if (key == null)
    {
        return Results.NotFound("API Key not found.");
    }

    key.IsActive = false;
    await db.SaveChangesAsync();

    return Results.Ok("API Key revoked successfully.");
});

app.MapGet("/products", async (AppDbContext db) => Results.Ok(await db.Products.ToListAsync())).RequireAuthorization();

app.Run();
