using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using NLog;
using NLog.Web;
using System.Globalization;
using System.Text;
using TelefonRehberi.Models;
using TelefonRehberi.Repositories;

var logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    var configuration = builder.Configuration;
    // NLog
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    // 🔌 GenericRepository Design Pattern
    builder.Services.AddScoped<IGenericRepository<Log>, GenericRepository<Log>>();
    builder.Services.AddScoped<IRehberRepository, RehberRepository>();
    builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // 🌍 Localization
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
    builder.Services.AddControllersWithViews()
           .AddViewLocalization()
           .AddDataAnnotationsLocalization();

    // 📦 DbContext
    builder.Services.AddDbContext<UygulamaDbContext>();
    //jwt giriş
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var jwtKey = jwtSettings["Key"];
    var jwtIssuer = jwtSettings["Issuer"];
    var jwtAudience = jwtSettings["Audience"];

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
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
        };
    });
    // 🌐 CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngular", policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // 📦 Swagger
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // 🌐 Kültür Ayarları
    var supportedCultures = new[] { "tr-TR", "en-US" };
    var localizationOptions = new RequestLocalizationOptions()
        .SetDefaultCulture("tr-TR")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
    app.UseRequestLocalization(localizationOptions);

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("AllowAngular");
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Uygulama çöktü!");
    throw;
}
finally
{
    LogManager.Shutdown();
}
