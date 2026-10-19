using business_layer;
using business_layer.Interfaces;
using data;
using data.Interfaces;
using Microsoft.EntityFrameworkCore;
using api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddDbContext<PaperlessDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));
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

if (app.Environment.IsDevelopment())
{
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI(); 
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PaperlessDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
