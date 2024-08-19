using System;
using System.Dynamic;

namespace NCS.DSS.Subscriptions.Helpers
{
    public interface IConvertToDynamic
    {
        public ExpandoObject ExcludeProperty(Exception exception, string[] names);

    }
}
