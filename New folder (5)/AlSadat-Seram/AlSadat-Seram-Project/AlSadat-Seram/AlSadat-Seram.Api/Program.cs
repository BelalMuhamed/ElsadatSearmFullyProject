using AlSadat_Seram.Api.Middlewares;
using AlSadat_Seram.Api.Responses;
using Application.Services.contract;
using Application.Services.contract.Authorization;
using Application.Services.contract.AuthService;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.JwtService;
using Application.Services.contract.LocalizationService;
using Domain.Common;
using Domain.Entities.Users;
using Domain.UnitOfWork.Contract;
using Infrastructure.Data;
using Infrastructure.Services;
using Infrastructure.Services.Authorization;
using Infrastructure.Services.AuthServices;
using Infrastructure.Services.CurrentUserServices;
using Infrastructure.Services.JwtServices;
using Infrastructure.Services.LocalizationServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
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
                .SetIsOriginAllowed(origin =>
                string.IsNullOrEmpty(origin) || // ✅ allow mobile apps
                allowedOrigins.Contains(origin)) // ✅ allow Angular
            .AllowAnyMethod()
            .AllowAnyHeader();
            });
        });
        #region Role-Based Authorization Policies

        // new
        #region Role-Based + Permission-Based Authorization Policies

        builder.Services.AddAuthorization(options =>
        {
            // Static/fixed catalog (Decision 5) — one named policy per EmployeePermissions
            // constant, registered explicitly rather than via a dynamic policy provider.
            var permissionCatalog = new Domain.Common.PermissionCatalog();
            foreach (var permission in permissionCatalog.AllPermissions)
            {
                options.AddPolicy(permission.Code, policy =>
                    policy.Requirements.Add(new Infrastructure.Authorization.PermissionRequirement(permission.Code)));
            }
        });
        builder.Services.AddSingleton<IAuthorizationHandler, Infrastructure.Authorization.PermissionAuthorizationHandler>();
        builder.Services.AddSingleton<Domain.Common.IPermissionCatalog, Domain.Common.PermissionCatalog>();

        builder.Host.UseDefaultServiceProvider(o =>
        {
            o.ValidateOnBuild = true;
            o.ValidateScopes = true;
        });

        builder.Services.AddControllers(options =>
        {
            options.Filters.Add(new AuthorizeFilter(
                new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build()));
            // C2: a user who must change the initial password can only change it or log out.
            options.Filters.Add<AlSadat_Seram.Api.Filters.PasswordChangeRequiredFilter>();
        })
        .AddControllersAsServices()
        // Same encoder MVC uses by default, so Arabic is not escaped in errors written outside MVC either.
        .AddJsonOptions(options =>
            options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping)
        .ConfigureApiBehaviorOptions(options =>
        {
            // Bare status results (415, NotFound()...) stay empty here and get the standard
            // envelope from the status code pages below.
            options.SuppressMapClientErrors = true;
            options.InvalidModelStateResponseFactory = context =>
            {
                var errorWriter = context.HttpContext.RequestServices.GetRequiredService<ApiErrorWriter>();
                return new BadRequestObjectResult(
                    errorWriter.CreateValidation(context.HttpContext, context.ModelState));
            };
        });
        #endregion
        #endregion

        #region Global CORS Policy
        builder.Services.AddIdentity<ApplicationUser,ApplicationRole>(options =>
        {

            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.AllowedForNewUsers = true;

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
                RoleClaimType = "role",
                NameClaimType = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });
        builder.Services.Configure<AuthSessionOptions>(builder.Configuration.GetSection("Auth"));
        #endregion
        QuestPDF.Settings.License = LicenseType.Community;
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddMemoryCache();
        builder.Services.AddSignalR();
        builder.Services.AddScoped(typeof(IUnitOfWork),typeof(Infrastructure.UnitOfWork.UnitOfWork));
        builder.Services.AddScoped(typeof(IExcelReaderService), typeof(ExcelReaderService));

        builder.Services.AddScoped(typeof(IServiceManager),typeof(ServiceManager));
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();
        builder.Services.AddScoped<IPermissionCatalogService, PermissionCatalogService>();
        builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
        builder.Services.AddScoped<IUserSessionService, UserSessionService>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ILocalizationService, LocalizationService>();
        builder.Services.AddScoped<ApiErrorWriter>();
        #region Global Rate Limiting
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, _) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                return ValueTask.CompletedTask;
            };
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
        await app.MigrateAndSeedDatabaseAsync();
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

                logger.LogInformation("🌐 API Request: {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                var hasAuthHeader = context.Request.Headers.ContainsKey("Authorization");
                logger.LogInformation("🔑 Auth header present: {HasAuthHeader}", hasAuthHeader);

                await next();

                logger.LogInformation("📤 API Response: {StatusCode} for {Path}",
                    context.Response.StatusCode, context.Request.Path);
            }
            else
            {
                await next();
            }
        });



        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        #region Global Exception Handling Middleware
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        // Gives the standard envelope to every empty error response (401, 403, 404, 405, 415, 429).
        app.UseStatusCodePages(context =>
        {
            var httpContext = context.HttpContext;
            var errorWriter = httpContext.RequestServices.GetRequiredService<ApiErrorWriter>();
            return errorWriter.WriteAsync(httpContext, (HttpStatusCode)httpContext.Response.StatusCode);
        });
        //app.UseHttpsRedirection();
        app.UseResponseCompression(); 
        app.UseStaticFiles();
        app.UseRouting();

        app.UseCors("AllowFrontend");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRateLimiter();

        app.MapControllers();
        // An unknown /api route must stay a JSON 404/405, not the single-page app.
        app.MapFallbackToFile("/", "index.html");
        app.MapFallbackToFile("{*path:nonfile:regex(^(?!api(/|$)).*$)}", "index.html");

        //app.MapHub<NotificationHub>("/notificationHub");
        #endregion

        app.Run();
    }
}