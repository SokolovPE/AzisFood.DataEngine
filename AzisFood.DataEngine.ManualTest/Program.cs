using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.ManualTest.Models;
using AzisFood.DataEngine.Postgres.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
     .AddPostgresOptions(builder.Configuration)
     .AddPostgresContext<CatalogDbContext>("catalog")
     .AddPostgresContext<PostgresDbContext>("postgres")
     .AddPostgresSupport();

var app = builder.Build();

app.MapGet("/", () =>
{
    // var context = app.Services.GetRequiredService<DbContext>();
    // var dbSet = context.Set<Category>();
    // var data = dbSet.AsNoTracking().ToList();
    
    // var dataAccess = app.Services.GetRequiredService<IDataAccess>();
    // var data = dataAccess.GetAllAsync<Category>().Result;

    var repoOrder = app.Services.GetRequiredService<IBaseRepository<Order>>();
    var zzz =repoOrder.CreateAsync(new Order {OrderDate = DateTime.Now.ToUniversalTime(), Price = 100.55m, Qty = 2}).Result;
    var dataa = repoOrder.GetAsync().Result;
    
    var repo = app.Services.GetRequiredService<IBaseRepository<Category>>();
    var z = repo.CreateAsync(new Category() {Order = 1, Title = "awfawf"}).Result;
    var data = repo.GetAsync().Result;
    
    return "Hello World!";
});

app.Run();
