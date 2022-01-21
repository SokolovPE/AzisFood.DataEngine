using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.ManualTest.Models;
using AzisFood.DataEngine.Postgres.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
     .AddPostgresOptions(builder.Configuration)
     .AddPostgresContext<PgDbContext>("root")
     .AddPostgresSupport();

var app = builder.Build();

app.MapGet("/", () =>
{
    // var context = app.Services.GetRequiredService<DbContext>();
    // var dbSet = context.Set<Category>();
    // var data = dbSet.AsNoTracking().ToList();
    
    // var dataAccess = app.Services.GetRequiredService<IDataAccess>();
    // var data = dataAccess.GetAllAsync<Category>().Result;

    var repo = app.Services.GetRequiredService<IBaseRepository<Category>>();
    var data = repo.GetAsync().Result;
    
    return "Hello World!";
});

app.Run();
