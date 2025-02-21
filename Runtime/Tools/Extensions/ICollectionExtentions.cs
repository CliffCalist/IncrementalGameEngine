using System;
using System.Collections.Generic;

namespace WhiteArrow.Incremental
{
    public static class ICollectionExtentions
    {
        public static ICollection<T> SelectByCondition<T>(this ICollection<T> collection, Func<T, bool> condition)
        {
            if (condition is null)
                throw new ArgumentNullException(nameof(condition));

            var output = new List<T>();
            foreach (var item in collection)
            {
                if (condition(item))
                    output.Add(item);
            }

            return output;
        }
    }
}