using System.Collections;

namespace AzisFood.DataEngine.Mongo.Tests.Helpers;

public static class HashCodeBuilder
{
    public static int Hash(params object[] args)
    {
        if (args == null) return 0;

        var num = 42;

        unchecked
        {
            foreach (var item in args)
                if (ReferenceEquals(item, null))
                {
                }
                else if (item.GetType().IsArray)
                {
                    foreach (var subItem in (IEnumerable) item) num = num * 37 + Hash(subItem);
                }
                else
                {
                    num = num * 37 + item.GetHashCode();
                }
        }

        return num;
    }
}