using System;
using System.Collections.Generic;
using System.Linq;

namespace DSLT4Support {

    public static class StringExtensions {

        private const string Separator = ",";

        /// <summary>
        /// Ensure that a potential 'expand' set is well formed
        /// </summary>
        /// <param name="src">raw source</param>
        /// <returns>Source unchanged or corrected</returns>
        public static string Normalize(this string src, bool caseInsensitiveParing = true) {
            return string.IsNullOrEmpty(src) ?
                    src :
                    string.Join(Separator, ExplodeExpansion(src, caseInsensitiveParing));
        }

        /// <summary>
        /// Parse a possible 'expand' specification into a populated or empty enumerable
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static IEnumerable<string> ExplodeExpansion(this string src, bool caseInsensitiveParing = true) {
            return string.IsNullOrWhiteSpace(src) ?
                        Enumerable.Empty<string>() :
                        src.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries).Sanitize(caseInsensitiveParing);
        }

        public static string CombineAsExpand(this IEnumerable<string> src, bool caseInsensitiveParing = true) {
            return string.Join(Separator, (src ?? Enumerable.Empty<string>()).Sanitize(caseInsensitiveParing));
        }

        private static IEnumerable<string> Sanitize(this IEnumerable<string> src, bool caseInsensitiveParing = true) {
            return src.Select(s => s.Trim())
                      .Where(s => !string.IsNullOrWhiteSpace(s))
                      .Distinct(caseInsensitiveParing ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        }

    }
}
