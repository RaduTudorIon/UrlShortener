
using Microsoft.EntityFrameworkCore;
using UrlShortener.Entities;
using UrlShortener.Extensions;
using UrlShortener.Models;
using UrlShortener.Services;

namespace UrlShortener;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("Database"));
        });

        builder.Services.AddScoped<UrlShorteningService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            app.ApplyMigrations();
        }

        app.MapPost("api/shorten", async (
                ShortenUrlRequest request,
                UrlShorteningService urlShorteningService,
                ApplicationDbContext dbContext,
                HttpContext httpContext) =>
        {
            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
            {
                return Results.BadRequest("Invalid URL");
            }

            var code = await urlShorteningService.GenerateUniqueCode();
            var shortenedUrl = new ShortenedUrl
            {
                Id = Guid.NewGuid(),
                LongUrl = request.Url,
                Code = code,
                ShortUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{code}",
                CreatedAtUtc = DateTime.UtcNow
            };

            dbContext.ShortenedUrls.Add(shortenedUrl);
            await dbContext.SaveChangesAsync();

            return Results.Created(shortenedUrl.ShortUrl, shortenedUrl);
        });

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
