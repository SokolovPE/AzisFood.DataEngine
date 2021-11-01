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
                Id = ObjectId.GenerateNewId().ToString(), LongField = new Random(2).Next(-100000, 100000),
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
            yield return new FakeEntity {Id = "6180614afb694666944bc5c5", LongField = -350, StringField = "text_1"};
            yield return new FakeEntity {Id = "6180614afb694666944bc5c7", LongField = -250, StringField = "text_2"};
            yield return new FakeEntity {Id = "6180614afb694666944bc5c9", LongField = -150, StringField = "text_3"};
            yield return new FakeEntity {Id = "6180614afb694666944bc5cb", LongField = -50, StringField = "text_4"};
            yield return new FakeEntity {Id = "6180614afb694666944bc5cd", LongField = -50, StringField = "text_5"};
            yield return new FakeEntity {Id = "6180614afb694666944bc5cf", LongField = 50, StringField = "text_6"};
            yield return new FakeEntity {Id = "6180614afb694666944bc5d1", LongField = 150, StringField = "text_7"};
            yield return new FakeEntity {Id = "6180614afb694666944bc5d3", LongField = 250, StringField = "text_8"};
            yield return new FakeEntity {Id = "6180614afb694666944bc5d5", LongField = 300, StringField = "text_9"};
            yield return new FakeEntity {Id = "6180614afb694666944bc5d7", LongField = 450, StringField = "text_10"};
        }
    }
}