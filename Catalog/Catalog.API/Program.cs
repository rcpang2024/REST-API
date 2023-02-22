using Catalog.API.Configuration;
using Catalog.API.Repositories;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Text.Json;
using System.Net.Mime;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // builder.Services.Configure<IMongoClient>(builder.Configuration);
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String)); // anytime it sees a Guid in any entity, it serializes them as a string in the database
        BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));
        var mongoDBsettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();

        builder.Services.AddSingleton<IMongoClient>(ServiceProvider =>
        {
            return new MongoClient(mongoDBsettings.ConnectionString);
        });
        builder.Services.AddSingleton<IItemsRepository, MongoDbItemsRepository>(); // registers the repository/dependency
        builder.Services.AddControllers(options =>
        {
            options.SuppressAsyncSuffixInActionNames = false;
        }); // prevents dotnet from removing the async suffix from any method at run time
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks()
            .AddMongoDb(
                mongoDBsettings.ConnectionString,
                name: "mongodb",
                timeout: TimeSpan.FromSeconds(5),
                tags: new[] { "ready" }); // adds health checks, based on if we can reach the db or not

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        if (app.Environment.IsDevelopment()) {
            app.UseHttpsRedirection();
        }

        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        { // makes sure we can use the service
            Predicate = (check) => check.Tags.Contains("ready"),
            ResponseWriter = async (context, report) => // specifies how to render the message you're getting as you get the result of the healthchecks
            {
                var result = JsonSerializer.Serialize( // collects the result
                    new { // anonymous type
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(entry => new { 
                            name = entry.Key,
                            status = entry.Value.Status.ToString(),
                            exception = entry.Value.Exception != null ? entry.Value.Exception.Message : "none",
                            duration = entry.Value.Duration.ToString()
                        })
                    }
                );
                context.Response.ContentType = MediaTypeNames.Application.Json; // formats the output to Json
                await context.Response.WriteAsync(result);
            }
        }); // endpoint that only includes health checks tagged with 'ready'

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        { // makes sure the service is running
            Predicate = (_) => false
        }); // excludes all health checks including the mongoDB one and comes back as long as the API is alive

        app.Run();
    }
}