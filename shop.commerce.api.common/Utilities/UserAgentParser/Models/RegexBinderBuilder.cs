namespace shop.commerce.api.common.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    internal static class RegexBinderBuilder
    {
        public static Func<Match, IEnumerator<int>, TResult> SelectMany<T1, T2, TResult>(
            this Func<Match, IEnumerator<int>, T1> binder,
            Func<T1, Func<Match, IEnumerator<int>, T2>> continuation,
            Func<T1, T2, TResult> projection)
        {
            return (m, num) =>
            {
                T1 bound = binder(m, num);
                T2 continued = continuation(bound)(m, num);
                TResult projected = projection(bound, continued);
                return projected;
            };
        }
    }
}
