# AzisFood.DataEngine
<img align="right" width="200" height="200" src="logo.png">

This library provides access to data from .NET application.

Library targets .NET 6.

Databases supported:
* MongoDb (using [MongoDB.Driver](https://docs.mongodb.com/drivers/csharp/))
* PostgreSQL (using [Entity Framework](https://docs.microsoft.com/ef/))

At the same time you can access multiple databases of same or different kinds.

# Project structure
  * **Abstractions** - Contains all common interfaces
  * **Core** - Common services and attributes which fit any database
  * **Mongo** - MongoDB specific services
  * **Postgres** - PostgreSQL specific services
  * **ManualTest** - Playground to test library manually
  * **Mongo.Tests** - Test of repository based on MongoDB

# Usage

## Application settings

First you need to configure your appsettings:
  * MongoDB
    ```json
    "MongoOptions": {
        "Connections": [
        {
            "Database": "your_awesome_mongo_database",
            "ConnectionName": "catalog",
            "ConnectionString": "mongodb://localhost:27017"
        },
        {
            "Database": "your_other_awesome_mongo_database",
            "ConnectionName": "service",
            "ConnectionString": "mongodb://localhost:27017"
        }
        ]
    }
    ```
  * PostgreSQL
    ```json
    "PgOptions": {
    "Connections": [
        {
        "ConnectionName": "catalog",
        "ConnectionString": "Server=localhost;Database=your_awesome_pg_database; User Id=root;Password=root"
        },
        {
        "ConnectionName": "service",
        "ConnectionString": "Server=localhost;Database=your_other_awesome_pg_database; User Id=root;Password=root"
        }
    ]
    }
    ```

## Registration
### Automated registration
If you want DataEngine to automatically discover available connections based on your appsettings and (in case of PostgreSQL) DbContexts
you can setup your Program.cs like that (choose methods which suits your desired databases):
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMongoSupport(builder.Configuration, new EngineConfiguration(true))
    .AddPostgresSupport(builder.Configuration, new EngineConfiguration(true));
```

```new EngineConfiguration(true)``` tells engine that it should work in auto-discover mode.

### Manual registration
If you want to setup your database connections manually - do not specify ```EngineConfiguration``` or provide ```false``` parameter:
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddMongoSupport(builder.Configuration)
    .AddPostgresSupport(builder.Configuration, new EngineConfiguration(true))
    .AddMongoConnect("catalog")
    .AddMongoConnect("service")
    .AddPostgresContext<CatalogDbContext>("catalog")
    .AddPostgresContext<PostgresDbContext>("service");
```

Method ```AddMongoConnect``` is used to register MongoDB connection, parameter should has corresponding ConnectionName in your appsettings.
Method ```AddPostgresContext``` is used to register PostgreSQL connection, generic parameters accepts your DbContext and regular parameter points to ConnectionName in your appsettings.

## Setup DbContext (PostgreSQL only)
To link DbContext to registered connection use ```ConnectionSettings``` attribute

**TODO: rename to "Alias"**
```csharp
[ConnectionSettings("catalog")]
public class CatalogDbContext : DbContext
{
    ...
}
```

## Setup entities
To link entity to registered connection use ```SupportedBy``` attribute to specify which database type this entity is stored at 
and use ```UseContext``` attribute to link entity to DbContext/Connection:

```csharp
[SupportedBy(DatabaseType.Postgres)]
[UseContext(nameof(CatalogDbContext))]
public class Category : PgRepoEntity
{
    ...
}

```

**TODO: get rid of SupportedBy attribute, it's possible to use parent class to determine which db type is it**

Depending on database type inherit your entity from:
* ```MongoRepoEntity``` - MongoDB
* ```PgRepoEntity``` - PostgreSQL

# Usage
When all preparations and configurations are done you can inject ```IBaseRepository<TEntity>``` and then use following methods
to access your data. Methods description can be found [here](https://github.com/SokolovPE/AzisFood.DataEngine/blob/master/AzisFood.DataEngine.Abstractions/Interfaces/IBaseRepository.cs)
