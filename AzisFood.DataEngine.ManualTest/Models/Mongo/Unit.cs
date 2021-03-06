using AzisFood.DataEngine.Core.Attributes;
using AzisFood.DataEngine.Mongo.Models;

#pragma warning disable CS8618

namespace AzisFood.DataEngine.ManualTest.Models.Mongo;

/// <summary>
///     Measurement unit
/// </summary>
[ConnectionAlias("mongo_service")]
public class Unit : MongoRepoEntity
{
    /// <summary>
    ///     Unit code
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    ///     Unit title
    /// </summary>
    public string Title { get; set; }
}