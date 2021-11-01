using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using AzisFood.DataEngine.Mongo.Tests.Models;
using Xunit.Sdk;

namespace AzisFood.DataEngine.Mongo.Tests.DataAttributes
{
    public class GetByExpressionData : DataAttribute
    {
        private readonly Expression<Func<FakeEntity, bool>> _longFieldGreaterThanZero = entity => entity.LongField > 0;
        private readonly Expression<Func<FakeEntity, bool>> _stringFieldContainsOne =
            entity => entity.StringField.Contains("1");
        private readonly Expression<Func<FakeEntity, bool>> _idStartsWithSixtyOne = entity => entity.Id.StartsWith("61");
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            yield return new object[] {_longFieldGreaterThanZero, 5};
            yield return new object[] {_stringFieldContainsOne, 2};
            yield return new object[] {_idStartsWithSixtyOne, 10};
        }
    }
}