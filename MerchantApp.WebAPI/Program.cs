using merchantapp.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using merchantapp.Infrastructure.Context;
using MerchantApp.WebAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<MerchantApplicationOptions>(builder.Configuration.GetSection("MerchantApplication"));
builder.Services.AddScoped<IMerchantApplicationService, MerchantApplicationService>();
builder.Services.AddHostedService<PendingApplicationCleanupService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "http://127.0.0.1:4200",
                "https://127.0.0.1:4200");
    });
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.WriteIndented = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

JwtOptions jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt ayarlari bulunamadi.");

if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey))
{
    throw new InvalidOperationException("Jwt:SecretKey bos olamaz.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; 
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();


var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception exception)
    {
        logger.LogWarning(exception, "Database migration could not be applied during startup.");
    }

    try
    {
        EnsurePortalUsersTable(dbContext);
    }
    catch (Exception exception)
    {
        logger.LogWarning(exception, "Portal users table bootstrap failed during startup.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static void EnsurePortalUsersTable(ApplicationDbContext dbContext)
{
    const string adminEmail = "admin@vakifbank.com";
    const string adminPasswordHash = "8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918";

    dbContext.Database.ExecuteSqlRaw(
        """
        IF OBJECT_ID(N'PortalUsers', N'U') IS NULL
        BEGIN
            CREATE TABLE [PortalUsers]
            (
                [Id] uniqueidentifier NOT NULL,
                [Email] varchar(150) NOT NULL,
                [PasswordHash] varchar(128) NOT NULL,
                [AccessToken] nvarchar(4000) NULL,
                [AccessTokenExpiresAtUtc] datetime2 NULL,
                [CreatedAt] datetime2 NOT NULL,
                CONSTRAINT [PK_PortalUsers] PRIMARY KEY ([Id])
            );

            CREATE UNIQUE INDEX [IX_PortalUsers_Email] ON [PortalUsers]([Email]);
        END;

        IF COL_LENGTH('PortalUsers', 'AccessToken') IS NULL
        BEGIN
            ALTER TABLE [PortalUsers] ADD [AccessToken] nvarchar(4000) NULL;
        END;

        IF COL_LENGTH('PortalUsers', 'AccessTokenExpiresAtUtc') IS NULL
        BEGIN
            ALTER TABLE [PortalUsers] ADD [AccessTokenExpiresAtUtc] datetime2 NULL;
        END;
        """);

    dbContext.Database.ExecuteSqlRaw(
        $"""
        IF NOT EXISTS (SELECT 1 FROM [PortalUsers] WHERE [Email] = '{adminEmail}')
        BEGIN
            INSERT INTO [PortalUsers] ([Id], [Email], [PasswordHash], [CreatedAt])
            VALUES (NEWID(), '{adminEmail}', '{adminPasswordHash}', SYSUTCDATETIME());
        END;
        """);
}
