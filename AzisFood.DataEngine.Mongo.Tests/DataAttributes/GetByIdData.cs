using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace AzisFood.DataEngine.Mongo.Tests.DataAttributes
{
    public class GetByIdData : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            yield return new object[] {Guid.Parse("61b85247-9695-43d2-b9e7-12a21d08cf28"), true};
            yield return new object[] {Guid.Empty, false};
            yield return new object[] {Guid.Parse("6177f023-b152-43db-88e6-fd1db184095b"), true};
            yield return new object[] {Guid.Parse("6177f023-b152-43db-88e6-fd1db184095c"), false};
        }
    }
}