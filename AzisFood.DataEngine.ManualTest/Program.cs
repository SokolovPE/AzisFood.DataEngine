using AzisFood.DataEngine.Core.Implementations;
using AzisFood.DataEngine.ManualTest.Models;
using AzisFood.DataEngine.Postgres.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPostgresOptions(builder.Configuration);
builder.Services
    .AddPostgresContext<PgDbContext>("root");
builder.Services
    .AddPostgresSupport();

var app = builder.Build();

app.MapGet("/", () =>
{
    var service = app.Services.GetRequiredService<BaseRepository<Category>>();
    var z = 2;
    return "Hello World!";
});

app.Run();
