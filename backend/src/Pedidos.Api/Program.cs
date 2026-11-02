using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Pedidos.Api;
using Pedidos.Application;
using Pedidos.Domain;
using Pedidos.Infrastructure;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddJsonConsole();
builder.Logging.AddProvider(new SerilogLoggerProvider(new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .Enrich.FromLogContext()
    .WriteTo.File(new JsonFormatter(renderMessage: true),
        Path.Combine(builder.Environment.ContentRootPath, "logs", "log-.txt"),
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: 10 * 1024 * 1024,
        rollOnFileSizeLimit: true,
        retainedFileCountLimit: 14,
        shared: true)
    .CreateLogger(), dispose: true));
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.Configure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = context =>
    new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState) { Title = "Revisa los datos enviados.", Status = 400 }));
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new();
if (Encoding.UTF8.GetByteCount(jwt.Key) < 32 || jwt.ExpiresIn is < 60 or > 86400)
    throw new InvalidOperationException("Configura Jwt__Key con al menos 32 bytes y una expiracion de 60 a 86400 segundos.");
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer, ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)), ClockSkew = TimeSpan.FromSeconds(15)
    };
});
builder.Services.AddAuthorization();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5173"])
    .AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddDbContext<PedidosDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("SqlServer") ?? throw new InvalidOperationException("Falta ConnectionStrings__SqlServer."),
    sql => sql.MigrationsAssembly("Pedidos.Infrastructure").EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)));
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Atlantic City | Pedidos", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new() { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" });
    options.AddSecurityRequirement(new() { [new() { Reference = new() { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = [] });
});
var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
else { app.UseHsts(); app.UseHttpsRedirection(); }
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Cache-Control"] = "no-store";
    await next();
});
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
if (app.Configuration.GetValue<bool>("Database:AutoMigrate"))
{
    using var scope = app.Services.CreateScope();
    var database = scope.ServiceProvider.GetRequiredService<PedidosDbContext>();
    await database.Database.MigrateAsync();
    var email = app.Configuration["Seed:Email"]?.Trim().ToLowerInvariant();
    var password = app.Configuration["Seed:Password"];
    if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password) && !await database.Usuarios.AnyAsync())
    {
        if (password.Length < 12) throw new InvalidOperationException("La clave inicial debe tener al menos 12 caracteres.");
        var user = new Usuario { Email = email, Rol = "Admin" };
        user.PasswordHash = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>().HashPassword(user, password);
        database.Usuarios.Add(user);
        await database.SaveChangesAsync();
    }
}
await app.RunAsync();

public partial class Program;