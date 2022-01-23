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
    "MongoConfiguration": {
      "AutoRegistration": true,
      "Connections": [
        {
          "Alias": "mongo_catalog",
          "ConnectionString": "mongodb://localhost:27017/your_awesome_database"
        },
        {
          "Alias": "mongo_service",
          "ConnectionString": "mongodb://localhost:27017/your_other_awesome_database"
        }
      ]
    }
    ```
  * PostgreSQL
    ```json
    "PgConfiguration": {
      "AutoRegistration": true,
      "Connections": [
        {
          "Alias": "catalog",
          "ConnectionString": "Server=localhost;Database=azisfood_catalog; User Id=root;Password=root"
        },
        {
          "Alias": "postgres",
          "ConnectionString": "Server=localhost;Database=postgres; User Id=root;Password=root"
        }
      ]
    }
    ```

If ```AutoRegistration``` set to ```true``` DataEngine will try to register all configured connections automatically.

## Registration
### Common step
To enable database support use specific method:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPostgresSupport(builder.Configuration)
    .AddMongoSupport(builder.Configuration);
```

Method ```AddMongoConnect``` is used to register MongoDB connection.

Method ```AddPostgresContext``` is used to register PostgreSQL connection.
### Automated registration
For automated registration set ```"AutoRegistration": true``` of your desired database type section of application config.

### Manual registration
If you want to setup your database connections manually - set ```"AutoRegistration": false``` of your desired database type section of application config.
 * MongoDB manual connection registration 

    To register MongoDB connect use ```AddMongoConnect(string alias)``` method and alias from your application settings as parameter.

example:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMongoSupport(builder.Configuration)
    .AddMongoConnect("mongo_catalog")
    .AddMongoConnect("mongo_service");
```
* PostgreSQL manual connection registration

  To register PostgreSQL connect use ```AddPostgresContext<TDbContext>()``` generic method and desired DbContext as generic parameter.

example:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPostgresSupport(builder.Configuration)
    .AddPostgresContext<CatalogDbContext>()
    .AddPostgresContext<PostgresDbContext>();
```

## Setup DbContext (PostgreSQL only)
To link DbContext to registered connection use ```ConnectionAlias``` attribute with value equal to one of specified in application settings.

```csharp
[ConnectionAlias("catalog")]
public class CatalogDbContext : DbContext
{
    ...
}
```

## Setup entities

### Common
Depending on database type inherit your entity from:
* ```MongoRepoEntity``` - MongoDB
* ```PgRepoEntity``` - PostgreSQL

### MongoDB
To link MongoDB entity to registered connection use ```ConnectionAlias``` attribute to specify which connection should be used:
```csharp
[ConnectionAlias("mongo_catalog")]
public class Category : MongoRepoEntity
{
    ...
}
```

### PostgreSQL
PostgreSQL entity is linked to one of specified DbContexts. Use ```UseContext``` attribute to link entity to DbContext:
```csharp
[UseContext(nameof(CatalogDbContext))]
public class Category : PgRepoEntity
{
    ...
}
```

# Usage
When all preparations and configurations are done you can inject ```IBaseRepository<TEntity>``` and then use following methods
to access your data. Methods description can be found [here](https://github.com/SokolovPE/AzisFood.DataEngine/blob/master/AzisFood.DataEngine.Abstractions/Interfaces/IBaseRepository.cs)
