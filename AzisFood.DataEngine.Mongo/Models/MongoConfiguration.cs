namespace AzisFood.DataEngine.Mongo.Models;

/// <summary>
///     MongoDB options
/// </summary>
public class MongoConfiguration
{
    /// <summary>
    ///     Automatically register all configured connections
    /// </summary>
    public bool AutoRegistration { get; set; } = true;
    
    /// <summary>
    ///     Connection configurations
    /// </summary>
    public MongoConnectConfiguration[] Connections { get; set; }
}