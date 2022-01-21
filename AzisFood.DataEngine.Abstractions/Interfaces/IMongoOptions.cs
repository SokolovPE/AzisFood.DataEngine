namespace AzisFood.DataEngine.Abstractions.Interfaces
{
    //TODO: Singleton instead?
    public interface IMongoOptions
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}