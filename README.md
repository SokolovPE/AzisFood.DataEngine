# AzisFood.DataEngine

<img align="right" width="200" height="200" src="logo.png">

This library provides access to data from .NET application.

Library targets .NET 6.

Databases supported:

- MongoDb (using [MongoDB.Driver](https://docs.mongodb.com/drivers/csharp/))
- PostgreSQL (using [Entity Framework](https://docs.microsoft.com/ef/))

At the same time you can access multiple databases of same or different kinds.

# Project structure

- **Abstractions** - Contains all common interfaces
- **Core** - Common services and attributes which fit any database
- **Mongo** - MongoDB specific services
- **Postgres** - PostgreSQL specific services
- **ManualTest** - Playground to test library manually
- **Mongo.Tests** - Test of repository based on MongoDB
- **NativeCacheAdapters** - Native implementation of ICacheAdapter for CachedBaseRepository
- **NativeCacheEventHandlers** - Native implementation of ICacheEventHandler for CachedBaseRepository

# Table of Contents

- [Application settings](#appsettings)
- [Registration](#registration)
  - [Common step](#registration-common)
  - [Automated registration](#registration-auto)
  - [Manual registration](#registration-manual)
- [Setup DbContext (PostgreSQL only)](#setup-dbcontext)
- [Setup entities](#setup-entities)
  - [Common](#setup-entities-common)
  - [MongoDB](#setup-entities-mongo)
  - [PostgreSQL](#setup-entities-postgre)
- [Usage](#usage)
- [Cached repository usage](#cached-repository)
  - [ICacheAdapter](#cache-adapter)
  - [ICacheEventHandler](#cache-event-handler)

# Usage

## <a name="appsettings"></a>Application settings

First you need to configure your appsettings:

- MongoDB
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
- PostgreSQL
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

If `AutoRegistration` set to `true` DataEngine will try to register all configured connections automatically.

## <a name="registration"></a>Registration

### <a name="registration-common"></a>Common step

To enable database support use specific method:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPostgresSupport(builder.Configuration)
    .AddMongoSupport(builder.Configuration);
```

Method `AddMongoConnect` is used to register MongoDB connection.

Method `AddPostgresContext` is used to register PostgreSQL connection.

### <a name="registration-auto"></a>Automated registration

For automated registration set `"AutoRegistration": true` of your desired database type section of application config.

### <a name="registration-manual"></a>Manual registration

If you want to setup your database connections manually - set `"AutoRegistration": false` of your desired database type section of application config.

- MongoDB manual connection registration

  To register MongoDB connect use `AddMongoConnect(string alias)` method and alias from your application settings as parameter.

example:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMongoSupport(builder.Configuration)
    .AddMongoConnect("mongo_catalog")
    .AddMongoConnect("mongo_service");
```

- PostgreSQL manual connection registration

  To register PostgreSQL connect use `AddPostgresContext<TDbContext>()` generic method and desired DbContext as generic parameter.

example:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPostgresSupport(builder.Configuration)
    .AddPostgresContext<CatalogDbContext>()
    .AddPostgresContext<PostgresDbContext>();
```

## <a name="setup-dbcontext"></a>Setup DbContext (PostgreSQL only)

To link DbContext to registered connection use `ConnectionAlias` attribute with value equal to one of specified in application settings.

```csharp
[ConnectionAlias("catalog")]
public class CatalogDbContext : DbContext
{
    ...
}
```

## <a name="setup-entities"></a>Setup entities

### <a name="setup-entities-common"></a>Common

Depending on database type inherit your entity from:

- `MongoRepoEntity` - MongoDB
- `PgRepoEntity` - PostgreSQL

### <a name="setup-entities-mongo"></a>MongoDB

To link MongoDB entity to registered connection use `ConnectionAlias` attribute to specify which connection should be used:

```csharp
[ConnectionAlias("mongo_catalog")]
public class Category : MongoRepoEntity
{
    ...
}
```

### <a name="setup-entities-postgre"></a>PostgreSQL

PostgreSQL entity is linked to one of specified DbContexts. Use generic parameter of `PgRepoEntity` base class to link entity to DbContext:

```csharp
public class Category : PgRepoEntity<CatalogDbContext>
{
    ...
}
```

# <a name="usage"></a>Usage

When all preparations and configurations are done you can inject `IBaseRepository<TEntity>` and then use following methods
to access your data. Methods description can be found [here](https://github.com/SokolovPE/AzisFood.DataEngine/blob/master/AzisFood.DataEngine.Abstractions/Interfaces/IBaseRepository.cs)

# <a name="cached-repository"></a>Cached repository usage

Library support usage of generic cached repositories.

Such repository uses cache as primary source and only if requested record was not found in cache looks for it in database.

It requires registered implementations of ICacheAdapter and ICacheEventHandler abstractions.

## <a name="cache-adapter"></a>ICacheAdapter

[This abstraction](https://github.com/SokolovPE/AzisFood.DataEngine/blob/master/AzisFood.DataEngine.Abstractions/Interfaces/ICacheAdapter.cs) is used to provide you ability to use any caching technology you want. All you need is implement this interface and register it as DI container.

All methods are required

Example of implementation can be found in [AzisFood.DataEngine.Cache.CacheService project](https://github.com/SokolovPE/AzisFood.DataEngine/tree/master/AzisFood.DataEngine.Cache.CacheService)

[AzisFood.CacheService.Redis](https://github.com/SokolovPE/AzisFood.CacheService) package used in this sample (W.I.P).

## <a name="cache-event-handler"></a>ICacheEventHandler

[This abstraction](https://github.com/SokolovPE/AzisFood.DataEngine/blob/master/AzisFood.DataEngine.Abstractions/Interfaces/ICacheEventHandler.cs) is used to provide you ability to react on following events during CRUD actions:

- Create: `NotifyCreate` called after record was created in database, created record passed as payload
- Update: `NotifyUpdate` called after record was updated in database, identifier and new value of record passed as parameters
- Delete: `NotifyRemove` called after record was deleted from database. In case of filtered deletion - there will be no data in parameters, in case of deletion by id (ids) identifiers will be passed as parameters
- Select: `NotifyMissing` called if required record (records) were not found in cache, but found in database. If record was requested by identifier - identifier will be passed as parameter, in other cases there will be no parameters (full cache refresh required event)

Example of implementation can be found in [AzisFood.DataEngine.MQ.Rabbit project](https://github.com/SokolovPE/AzisFood.DataEngine/tree/master/AzisFood.DataEngine.MQ.Rabbit)

[AzisFood.MQ.Rabbit](https://github.com/SokolovPE/AzisFood.MQ) package used in this sample (W.I.P).
