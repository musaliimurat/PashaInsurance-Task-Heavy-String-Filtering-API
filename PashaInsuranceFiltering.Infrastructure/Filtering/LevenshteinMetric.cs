using PashaInsuranceFiltering.Application.Common.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.Infrastructure.Filtering
{
    public sealed class LevenshteinMetric : ISimilarityMetric
    {
        public double Similarity(string a, string b)
        {
            if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase)) return 1.0;
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0.0;

            a = a.ToLowerInvariant(); b = b.ToLowerInvariant();

            var m = a.Length; var n = b.Length;
            var d = new int[m + 1, n + 1];
            for (int i = 0; i <= m; i++) d[i, 0] = i;
            for (int j = 0; j <= n; j++) d[0, j] = j;

            for (int i = 1; i <= m; i++)
                for (int j = 1; j <= n; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }

            var dist = d[m, n];
            var maxLen = Math.Max(m, n);
            return 1.0 - (double)dist / maxLen; 
        }
    }

}
