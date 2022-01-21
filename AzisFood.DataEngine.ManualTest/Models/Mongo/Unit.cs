using AzisFood.DataEngine.Core;
using AzisFood.DataEngine.Core.Attributes;
using AzisFood.DataEngine.Mongo.Models;

namespace AzisFood.DataEngine.ManualTest.Models.Mongo;

/// <summary>
/// Measurement unit
/// </summary>
[SupportedBy(DatabaseType.Mongo)]
[UseContext("service")]
public class Unit : MongoRepoEntity
{
    /// <summary>
    /// Unit code
    /// </summary>
    public string Code { get; set; }
    
    /// <summary>
    /// Unit title
    /// </summary>
    public string Title { get; set; }
}