namespace AzisFood.DataEngine.Mongo.Models;

/// <summary>
///     MongoDB options
/// </summary>
public class MongoConfiguration
{
    public MongoConnectConfiguration[] Connections { get; set; }
}