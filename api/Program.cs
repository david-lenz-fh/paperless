using api.Middleware;
using business_layer;
using business_layer.Interfaces;
using data;
using data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Minio;

var builder = WebApplication.CreateBuilder(args);

// MinIO
var minioEndpoint = builder.Configuration["Minio:Endpoint"] ?? "minio:9000";
var minioAccessKey = builder.Configuration["Minio:AccessKey"] ?? "minioadmin";
var minioSecretKey = builder.Configuration["Minio:SecretKey"] ?? "minioadminpassword";
var minioUseSSL = builder.Configuration.GetValue<bool>("Minio:UseSSL");

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Controller & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IFileStorageService, MinioFileStorageService>();

// MinIO Client
builder.Services.AddMinio(configureClient => configureClient
    .WithEndpoint(minioEndpoint)
    .WithCredentials(minioAccessKey, minioSecretKey)
    .WithSSL(minioUseSSL)
    .Build());

// DB
builder.Services.AddDbContext<PaperlessDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// DB-Migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PaperlessDbContext>();
    dbContext.Database.Migrate();
}

app.Run();