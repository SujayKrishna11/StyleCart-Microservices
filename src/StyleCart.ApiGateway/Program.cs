using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var ocelotFileName =
    Environment.GetEnvironmentVariable("RUNNING_IN_CONTAINER") == "true"
        ? "ocelot.Container.json"
        : "ocelot.json";

builder.Configuration.AddJsonFile(
    ocelotFileName,
    optional: false,
    reloadOnChange: true);

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT audience is missing.");

builder.Services.AddAuthentication()
    .AddJwtBearer("GatewayJwt", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api/support"),
    supportApi =>
    {
        supportApi.UseRouting();
        supportApi.UseAuthentication();
        supportApi.UseAuthorization();
        supportApi.UseEndpoints(endpoints => endpoints.MapControllers());
    });

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
await app.UseOcelot();

app.Run();
