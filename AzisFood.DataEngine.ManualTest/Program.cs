using System.Text.Json;
using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Mongo.Extensions;
using AzisFood.DataEngine.Postgres.Extensions;
using Microsoft.EntityFrameworkCore;
using MongoCategory = AzisFood.DataEngine.ManualTest.Models.Mongo.Category;
using MongoUnit = AzisFood.DataEngine.ManualTest.Models.Mongo.Unit;
using PostgresCategory = AzisFood.DataEngine.ManualTest.Models.Postgres.Category;
using PostgresOrder = AzisFood.DataEngine.ManualTest.Models.Postgres.Order;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPostgresSupport(builder.Configuration)
    .AddMongoSupport(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/mongo", async () =>
{
    var unitRepo = app.Services.GetRequiredService<IBaseRepository<MongoUnit>>();
    var units = await unitRepo.GetAsync(false);

    var categoryRepo = app.Services.GetRequiredService<IBaseRepository<MongoCategory>>();
    var categories = await categoryRepo.GetAsync(false);

    var result = new {Units = units, Categories = categories};
    return JsonSerializer.Serialize(result);
});

app.MapGet("/pg", async () =>
{
    // var context = app.Services.GetRequiredService<DbContext>();
    // var dbSet = context.Set<Category>();
    // var data = dbSet.AsNoTracking().ToList();

    // var dataAccess = app.Services.GetRequiredService<IDataAccess>();
    // var data = dataAccess.GetAllAsync<Category>().Result;

    var repoOrder = app.Services.GetRequiredService<IBaseRepository<PostgresOrder>>();
    await repoOrder.CreateAsync(new PostgresOrder
        {OrderDate = DateTime.Now.ToUniversalTime(), Price = 100.55m, Qty = 2});
    var orders = await repoOrder.GetAsync(false);

    var repoCategory = app.Services.GetRequiredService<IBaseQueryableRepository<PostgresCategory>>();
    //await repoCategory.CreateAsync(new PostgresCategory {Order = 1, Title = "awfawf"});
    var categories = repoCategory.GetQueryable(false);
    var list = await categories.ToListAsync();

    var result = new {Orders = orders, Categories = list};
    return JsonSerializer.Serialize(result);
});

app.Run();