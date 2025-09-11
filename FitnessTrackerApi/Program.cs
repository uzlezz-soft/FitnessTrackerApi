using FitnessTrackerApi.Configs;
using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Endpoints;
using FitnessTrackerApi.Exceptions;
using FitnessTrackerApi.Models;
using FitnessTrackerApi.Services;
using FitnessTrackerApi.Services.Auth;
using FitnessTrackerApi.Services.Workout;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// https://stackoverflow.com/a/76644093
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContextPool<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
});

builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Auth:Issuer"],
            ValidAudience = builder.Configuration["Auth:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Auth:Key"]!))
        };
    });

builder.Services.Configure<AuthConfig>(
    builder.Configuration.GetSection(AuthConfig.SectionName));
builder.Services.Configure<ImagesConfig>(
    builder.Configuration.GetSection(ImagesConfig.SectionName));
builder.Services.Configure<FileSystemImageRepositoryConfig>(
    builder.Configuration.GetSection(FileSystemImageRepositoryConfig.SectionName));
builder.Services.Configure<CacheConfig>(
    builder.Configuration.GetSection(CacheConfig.SectionName));

builder.Services.AddScoped<ITokenProvider, TokenProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddSingleton<IImageRepository, FileSystemImageRepository>();
builder.Services.AddScoped<IPhotoService, PhotoService>();

builder.Services.AddScoped<IWorkoutService, WorkoutService>();

builder.Services.AddHostedService<RefreshTokenCleanupHostedService>();

builder.Services.AddValidatorsFromAssemblyContaining<UserLoginValidator>();

builder.Services.AddAuthorization();

builder.Services.AddExceptionHandler<ExceptionHandler>();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 30,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});

builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "create-or-refresh", options =>
    {
        options.PermitLimit = 3;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    }));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseExceptionHandler(_ => { });

app.RegisterUserEndpoints();
app.RegisterWorkoutEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();