using System.Numerics;
using System.Text;
using forecast_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

const string LocalFrontendCorsPolicy = "_localFrontendCorsPolicy";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddScoped<IAuthenticationService, DummyAuthenticationService>();
builder.Services.AddScoped<ICityService, InMemoryCityService>();
builder.Services.AddScoped<IIPGeolocationService, IPAPI_IPGeolocationService>();
builder.Services.AddScoped<ICityDataService, WikipediaCityDataService>();
builder.Services.AddScoped<IForecastService, OpenMeteoForecastService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
    };
});



builder.Services.AddCors(options =>
{
    options.AddPolicy(name: LocalFrontendCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader().AllowCredentials();
        // policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(LocalFrontendCorsPolicy);

app.UseHttpsRedirection();



app.UseAuthorization();
app.MapControllers();

app.Run();
