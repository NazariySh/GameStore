using Gamestore.BLL;
using Gamestore.DAL;
using Gamestore.WebApi.Extensions;
using Gamestore.WebApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services
    .AddDataAccessLayer(builder.Configuration)
    .AddBusinessLogicLayer(builder.Configuration);

builder.Services.AddMapping();

builder.Services.AddCustomMiddlewares();

builder.Services.AddOutputCachingPolicy(builder.Configuration);

builder.Services.AddCorsPolicy(builder.Configuration);

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

builder.Host.ConfigureSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    await app.InitializeAsync();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<TotalGamesHeaderMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.UseStaticFiles();

app.MapControllers();

await app.RunAsync();