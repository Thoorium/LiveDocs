using System.Collections.Generic;
using System.Linq;

namespace LiveDocs.Shared
{
    public static class CollectionHelper
    {
        /// <summary>
        /// Create the cartesian product between two <c>IEnumerable</c> elements, without duplicates.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Source: https://stackoverflow.com/questions/25643382/cartesian-products-with-n-number-of-list/25643434 </remarks>
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct =
              new[] { Enumerable.Empty<T>() };
            IEnumerable<IEnumerable<T>> result = emptyProduct;
            foreach (IEnumerable<T> sequence in sequences)
            {
                result = from accseq in result from item in sequence select accseq.Concat(new[] { item });
            }
            return result;
        }
    }
}