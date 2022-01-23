#nullable enable
using AzisFood.DataEngine.Core.Attributes;
using AzisFood.DataEngine.Mongo.Models;
using AzisFood.DataEngine.Mongo.Tests.Helpers;

namespace AzisFood.DataEngine.Mongo.Tests.Models;

/// <summary>
///     Fake entity for testing
/// </summary>
[UseContext("fake-connection")]
public class FakeEntity : MongoRepoEntity
{
    public string StringField { get; set; } = null!;
    public long LongField { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is FakeEntity other && Equals(other);
    }

    private bool Equals(FakeEntity other)
    {
        return other.Id == Id && other.StringField == StringField && other.LongField == LongField;
    }

    public override int GetHashCode()
    {
        return HashCodeBuilder.Hash(Id, StringField, LongField);
    }

    public override string ToString()
    {
        return $"Id={Id}, StringField={StringField}, LongField={LongField}";
    }
}