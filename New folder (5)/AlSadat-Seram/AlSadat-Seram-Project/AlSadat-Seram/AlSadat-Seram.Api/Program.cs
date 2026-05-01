using AlSadat_Seram.Api.Middlewares;
using Application.Mappings;
using Application.Services.contract;
using Domain.Common;
using Domain.Entities.Users;
using Domain.UnitOfWork.Contract;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

        // Add services to the container.

        builder.Services.AddDbContext<AppDbContext>(options =>
          options.UseSqlServer(builder.Configuration.GetConnectionString("con")));


        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
        #region Role-Based Authorization Policies

        builder.Services.AddAuthorization();
        builder.Services.AddControllers(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                             .RequireAuthenticatedUser()
                             .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });
        #endregion
        #region Global CORS Policy
        builder.Services.AddIdentity<ApplicationUser,ApplicationRole>(options =>
        {

            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });
        #endregion

        #region Swagger with JWT Authentication
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1",new OpenApiInfo { Title = "AlSadat Seram Project API",Version = "v1" });

            c.AddSecurityDefinition("Bearer",new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
            });

        });
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        builder.Services.Configure<JwtSettings>(jwtSettings);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings["Key"])),

                RoleClaimType = ClaimTypes.Role,   
                NameClaimType = ClaimTypes.NameIdentifier
            };
        });
        //.AddJwtBearer(options =>
        // {
        //     options.TokenValidationParameters = new TokenValidationParameters
        //     {
        //         ValidateIssuer = true,
        //         ValidateAudience = true,
        //         ValidateLifetime = true,
        //         ValidateIssuerSigningKey = true,
        //         ValidIssuer = jwtSettings["Issuer"],
        //         ValidAudience = jwtSettings["Audience"],
        //         IssuerSigningKey = new SymmetricSecurityKey(
        //             Encoding.UTF8.GetBytes(jwtSettings["Key"]))
        //     };
        // });
        #endregion
        QuestPDF.Settings.License = LicenseType.Community;
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddMemoryCache();
        builder.Services.AddSignalR();
        builder.Services.AddScoped(typeof(IUnitOfWork),typeof(Infrastructure.UnitOfWork.UnitOfWork));
        builder.Services.AddScoped(typeof(IExcelReaderService), typeof(ExcelReaderService));

        builder.Services.AddScoped(typeof(IServiceManager),typeof(ServiceManager));

        #region Global Rate Limiting
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext,string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(

                    partitionKey: httpContext.Connection.RemoteIpAddress!.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }));
        });
        #endregion

        var app = builder.Build();
        await app.SeedDatabaseAsync();

        app.Use(async (context,next) =>
        {
            // تسجيل كل طلب API
            if(context.Request.Path.StartsWithSegments("/api"))
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

                // تسجيل معلومات الطلب
                logger.LogInformation("🌐 API Request: {Method} {Path}",
                    context.Request.Method,context.Request.Path);

                var authHeader = context.Request.Headers["Authorization"].ToString();

                logger.LogInformation("🔑 Auth Header: {Header}",
                    string.IsNullOrEmpty(authHeader)
                        ? "EMPTY"
                        : authHeader.Length > 50
                            ? authHeader.Substring(0, 50) + "..."
                            : authHeader);

                await next();

                // تسجيل الرد
                logger.LogInformation("📤 API Response: {StatusCode} for {Path}",
                    context.Response.StatusCode,context.Request.Path);
            }
            else
            {
                await next();
            }
        });



        // Configure the HTTP request pipeline.
        if(app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        #region Global Exception Handling Middleware
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        //app.UseHttpsRedirection();
        app.UseResponseCompression(); 
        app.UseStaticFiles();
        app.UseRouting();

        app.UseCors("AllowFrontend");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRateLimiter();

        app.MapControllers();
        app.MapFallbackToFile("index.html");

        //app.MapHub<NotificationHub>("/notificationHub");
        #endregion

        app.Run();
    }
}