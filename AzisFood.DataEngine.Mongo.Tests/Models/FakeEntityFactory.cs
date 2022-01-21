using System;
using System.Collections.Generic;
using AzisFood.DataEngine.Mongo.Tests.Helpers;
using MongoDB.Bson;

namespace AzisFood.DataEngine.Mongo.Tests.Models
{
    /// <summary>
    /// Factory of fake entities
    /// </summary>
    public static class FakeEntityFactory
    {
        /// <summary>
        /// Create single fake entity
        /// </summary>
        /// <returns>Instance of fake entity</returns>
        public static FakeEntity CreateRandom() =>
            new()
            {
                Id = Guid.NewGuid(), LongField = new Random(2).Next(-100000, 100000),
                StringField = StringHelper.RandomString(50)
            };

        /// <summary>
        /// Crate IEnumerable of fake entities
        /// </summary>
        /// <param name="count">Number of entities to create</param>
        /// <returns>IEnumerable of fake entities</returns>
        public static IEnumerable<FakeEntity> CreateManyRandom(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return CreateRandom();
            }
        }

        /// <summary>
        /// Return IEnumerable of pre-made fake entities
        /// </summary>
        /// <returns>Pre-made IEnumerable of fake entities</returns>
        public static IEnumerable<FakeEntity> CreatePremade()
        {
            yield return new FakeEntity {Id = Guid.Parse("61b85247-9695-43d2-b9e7-12a21d08cf28"), LongField = -350, StringField = "text_1"};
            yield return new FakeEntity {Id = Guid.Parse("613119c4-1b47-42a0-84b2-ddc9dc7de6b4"), LongField = -250, StringField = "text_2"};
            yield return new FakeEntity {Id = Guid.Parse("61a405f0-963a-4be1-8d9d-d2d66f468deb"), LongField = -150, StringField = "text_3"};
            yield return new FakeEntity {Id = Guid.Parse("61b0d515-b8f8-4878-9f9b-714bade6925a"), LongField = -50, StringField = "text_4"};
            yield return new FakeEntity {Id = Guid.Parse("6177f023-b152-43db-88e6-fd1db184095b"), LongField = -50, StringField = "text_5"};
            yield return new FakeEntity {Id = Guid.Parse("61b1d880-b4a4-4210-8438-c676117c616b"), LongField = 50, StringField = "text_6"};
            yield return new FakeEntity {Id = Guid.Parse("618e6aed-276e-4c4c-8639-4b78c6f863d0"), LongField = 150, StringField = "text_7"};
            yield return new FakeEntity {Id = Guid.Parse("61426123-3493-4b48-8718-407a5a23c782"), LongField = 250, StringField = "text_8"};
            yield return new FakeEntity {Id = Guid.Parse("61d6cf62-dca0-4dff-b7e6-67b78dbe69ad"), LongField = 300, StringField = "text_9"};
            yield return new FakeEntity {Id = Guid.Parse("61b51ae9-bb86-401f-a825-17fbfafd7770"), LongField = 450, StringField = "text_10"};
        }
    }
}