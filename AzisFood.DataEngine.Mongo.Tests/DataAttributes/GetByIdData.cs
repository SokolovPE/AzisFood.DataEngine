using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace AzisFood.DataEngine.Mongo.Tests.DataAttributes
{
    public class GetByIdData : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            yield return new object[] {"6180614afb694666944bc5c5", true};
            yield return new object[] {"magical", false};
            yield return new object[] {"6180614afb694666944bc5d1", true};
            yield return new object[] {"9180614afb694666a44bc5zj", false};
        }
    }
}