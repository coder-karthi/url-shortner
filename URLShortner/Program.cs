using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using URLShortner.Contracts;
using URLShortner.Data;
using URLShortner.Repositories;
using URLShortner.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnectionString")));
builder.Services.AddScoped<Base62Service>();
builder.Services.AddScoped<UrlService>();
builder.Services.AddScoped<UrlMappingRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Urls.Add("http://0.0.0.0:8080");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.MapPost("/api/url", async (string longUrl, UrlService service, HttpContext httpContext) =>
{
    if (string.IsNullOrWhiteSpace(longUrl))
        return Results.BadRequest("Invalid URL");

    var shortCode = await service.CreateShortUrl(longUrl);

    var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";

    return Results.Ok(new
    {
        shortUrl = $"{baseUrl}/{shortCode}"
    });
});

app.MapGet("/api/url", async ([AsParameters] UrlMappingListQuery query, UrlService service) =>
{
    var allowedSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "createdAt",
        "clickCount",
        "shortCode",
        "longUrl"
    };

    if (!allowedSortFields.Contains(query.SortBy))
    {
        return Results.BadRequest(new
        {
            message = "Invalid sortBy value. Supported values: createdAt, clickCount, shortCode, longUrl."
        });
    }

    if (!string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase))
    {
        return Results.BadRequest(new
        {
            message = "Invalid sortDirection value. Supported values: asc, desc."
        });
    }

    var result = await service.GetUrlMappings(query);
    return Results.Ok(result);
});

app.MapGet("/{shortCode}", async (string shortCode, UrlService service) =>
{
    var url = await service.GetByShortCode(shortCode);

    if (url == null)
        return Results.NotFound();

    await service.IncrementClick(shortCode);

    return Results.Redirect(url.LongUrl);
});

app.MapGet("/api/url/{shortCode}", async (string shortCode, UrlService service) =>
{
    var url = await service.GetByShortCode(shortCode);

    if (url == null)
        return Results.NotFound();

    return Results.Ok(new
    {
        url.LongUrl,
        url.ClickCount,
        url.CreatedAt
    });
});

app.Run();
