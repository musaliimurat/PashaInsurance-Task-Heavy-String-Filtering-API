using PashaInsuranceFiltering.Application.Common.Ports;

namespace PashaInsuranceFiltering.Infrastructure.Filtering
{
    public sealed class JaroWinklerMetric : ISimilarityMetric
    {
        public double Similarity(string a, string b)
        {
            if (a == null || b == null)
                return 0.0;

            if (a.Equals(b, StringComparison.OrdinalIgnoreCase))
                return 1.0;

            // Lowercase (case-insensitive)
            a = a.ToLowerInvariant();
            b = b.ToLowerInvariant();

            int aLen = a.Length;
            int bLen = b.Length;

            if (aLen == 0 || bLen == 0)
                return 0.0;

            int matchDistance = Math.Max(aLen, bLen) / 2 - 1;
            if (matchDistance < 0) matchDistance = 0;

            bool[] aMatches = new bool[aLen];
            bool[] bMatches = new bool[bLen];

            int matches = 0;
            for (int i = 0; i < aLen; i++)
            {
                int start = Math.Max(0, i - matchDistance);
                int end = Math.Min(i + matchDistance + 1, bLen);

                for (int j = start; j < end; j++)
                {
                    if (bMatches[j]) continue;
                    if (a[i] != b[j]) continue;

                    aMatches[i] = true;
                    bMatches[j] = true;
                    matches++;
                    break;
                }
            }

            if (matches == 0) return 0.0;

           
            double t = 0;
            int k = 0;
            for (int i = 0; i < aLen; i++)
            {
                if (!aMatches[i]) continue;
                while (!bMatches[k]) k++;
                if (a[i] != b[k]) t++;
                k++;
            }
            t /= 2.0;

           
            double m = matches;
            double jaro = ((m / aLen) + (m / bLen) + ((m - t) / m)) / 3.0;

            int prefix = 0;
            for (int i = 0; i < Math.Min(4, Math.Min(aLen, bLen)); i++)
            {
                if (a[i] == b[i]) prefix++;
                else break;
            }

            double jaroWinkler = jaro + (prefix * 0.1 * (1 - jaro));
            return jaroWinkler > 1.0 ? 1.0 : jaroWinkler;
        }
    }
}

