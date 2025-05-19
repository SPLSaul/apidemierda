using System.Reflection;
using apiDDKMA.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuración CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configuración Swagger simplificada
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DDKMA API",
        Version = "v1",
        Description = "API para la aplicación DDKMA"
    });

    c.UseInlineDefinitionsForEnums();
    c.UseAllOfToExtendReferenceSchemas();
    c.SupportNonNullableReferenceTypes();

    // Documentación XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Registro de servicios
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PastelService>();
builder.Services.AddScoped<CarritoService>();

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configuración del pipeline de solicitudes HTTP
app.UseExceptionHandler("/error");
app.Map("/error", (HttpContext context) =>
{
    var exceptionHandler = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
    var exception = exceptionHandler?.Error;
    return Results.Problem(
        detail: exception?.Message,
        statusCode: StatusCodes.Status500InternalServerError
    );
});

app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DDKMA API v1");
    c.RoutePrefix = "swagger";
    c.ConfigObject.DisplayRequestDuration = true;
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapControllers();
app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();