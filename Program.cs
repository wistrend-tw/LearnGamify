
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using LearnGamify.Models;
using LearnGamify.Services;
using LearnGamify.Services.Base;

var builder = WebApplication.CreateBuilder(args);

ConfigureDatabase(builder);
ConfigureJson(builder);
ConfigureSession(builder);
ConfigureCors(builder);
ConfigureAuthentication(builder);
ConfigureServices(builder);

ConfigureMiddleware(builder);



// --------------------- 方法封裝區 ---------------------

void ConfigureDatabase(WebApplicationBuilder builder)
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<LearnGamifyContext>(options =>
        options.UseMySQL(connectionString)
               .LogTo(Console.WriteLine, LogLevel.Information)
               .EnableSensitiveDataLogging());
}

void ConfigureJson(WebApplicationBuilder builder)
{
    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs);
    });

    builder.Services.Configure<JsonSerializerOptions>(options =>
    {
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs);
    });
}

void ConfigureSession(WebApplicationBuilder builder)
{
    builder.Services.AddDistributedMemoryCache();

    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("Site:SessionTimeOut"));
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;

        if (builder.Configuration.GetValue<bool>("Site:SSL"))
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

    builder.Services.Configure<CookiePolicyOptions>(options =>
    {
        options.CheckConsentNeeded = context => true;
        options.MinimumSameSitePolicy = SameSiteMode.None;
    });
}

void ConfigureCors(WebApplicationBuilder builder)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowMyOrigin", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });
}

void ConfigureAuthentication(WebApplicationBuilder builder)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.IncludeErrorDetails = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["JwtSettings:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"])),
                TokenDecryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Encryptkey"]))
            };
        });
}

void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<Service_Member>();
    builder.Services.AddScoped<ServiceBase_Authorization>();
    builder.Services.AddScoped<ServiceBase_Database>();

    // 引入系統提供 Service
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddAuthorization();
    builder.Services.AddControllers();
    builder.Services.AddHttpClient();
}

void ConfigureMiddleware(WebApplicationBuilder builder)
{
    var app = builder.Build();
    app.UseAuthentication();
    app.UseAuthorization(); // 如有授權
    app.UseCors("AllowMyOrigin");
    app.UseStaticFiles(); // 若您要服務靜態圖片
    app.UseRouting();
    app.UseSession();
    app.UseHttpsRedirection();
    app.MapControllers();
    app.Run();
}

