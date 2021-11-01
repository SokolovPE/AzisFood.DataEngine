using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AzisFood.DataEngine.Mongo.Tests.Models
{
    public class FakeEntityFixture
    {
        public List<FakeEntity> Items { get; set; } = FakeEntityFactory.CreatePremade().ToList();
    }
}