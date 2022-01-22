using AzisFood.DataEngine.Core;
using AzisFood.DataEngine.Core.Attributes;
using AzisFood.DataEngine.Mongo.Models;
#pragma warning disable CS8618

namespace AzisFood.DataEngine.ManualTest.Models.Mongo;

/// <summary>
///     Model of product category
/// </summary>
[SupportedBy(DatabaseType.Mongo)]
[UseContext("catalog")]
public class Category : MongoRepoEntity
{
    /// <summary>
    ///     Title of category
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     Possible subcategories
    /// </summary>
    public Guid[] SubCategories { get; set; }

    /// <summary>
    ///     Order of category
    /// </summary>
    public int Order { get; set; }
}