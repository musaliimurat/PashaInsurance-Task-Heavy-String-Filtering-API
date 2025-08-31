using System.Text;
using System.Text.RegularExpressions;
using PashaInsuranceFiltering.Application.Common.Ports;

namespace PashaInsuranceFiltering.Infrastructure.Filtering;

public sealed class InMemoryTextFilter : ITextFilter
{
    private static readonly Regex Tokenizer =
        new Regex(@"(\s+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly string[] _bannedTokens;
    private readonly (Regex regex, string raw)[] _bannedPhrases;

    private readonly ISimilarityMetric _metric;

    public InMemoryTextFilter(string[] bannedWords, ISimilarityMetric metric)
    {
        _metric = metric;

        var list = (bannedWords ?? Array.Empty<string>())
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .Select(w => w.Trim());

        _bannedPhrases = list.Where(w => w.Contains(' ', StringComparison.Ordinal))
            .Select(raw =>
            {
                var parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var escaped = parts.Select(Regex.Escape);

                var pattern = $@"(?<!\w){string.Join(@"\W+", escaped)}(?!\w)\p{{P}}*";
                var re = new Regex(
                    pattern,
                    RegexOptions.Compiled |
                    RegexOptions.IgnoreCase |
                    RegexOptions.CultureInvariant |
                    RegexOptions.Singleline
                );
                return (re, raw);
            })
            .ToArray();

        _bannedTokens = list.Where(w => !w.Contains(' ', StringComparison.Ordinal))
            .Select(NormalizeToken)
            .ToArray();
    }

    public string Filter(string input, double threshold)
    {
        if (string.IsNullOrEmpty(input))
            return input ?? string.Empty;

        threshold = Clamp01(threshold);

        foreach (var (re, _) in _bannedPhrases)
            input = re.Replace(input, " "); 

        var tokens = Tokenizer.Split(input);
        if (tokens.Length == 0) return input;

        var remove = new bool[tokens.Length];

        Parallel.For(0, tokens.Length, i =>
        {
            var t = tokens[i];
            if (string.IsNullOrWhiteSpace(t)) return;

            var trimmed = TrimPunctuation(t);
            var norm = NormalizeToken(trimmed);

            if (string.IsNullOrEmpty(norm)) { remove[i] = true; return; }

            foreach (var bad in _bannedTokens)
            {
                double sim;
                try { sim = _metric.Similarity(norm, bad); }
                catch { return; }

                if (sim >= threshold)
                {
                    remove[i] = true;
                    break;
                }
            }
        });

        var sb = new StringBuilder(input.Length);
            for (int i = 0; i < tokens.Length; i++)
            {
                if (!remove[i] && !string.IsNullOrWhiteSpace(tokens[i]))
                {
                    sb.Append(tokens[i] + " ");
                    if (i < tokens.Length - 1 && !remove[i + 1] && !string.IsNullOrWhiteSpace(tokens[i + 1]))
                    {
                        sb.Append(' ');
                    }
                }
            }
        return sb.ToString().Trim();
    }

    public Task<string> FilterAsync(string input, double threshold, CancellationToken ct = default)
        => Task.Run(() => Filter(input, threshold), ct);

    private static string TrimPunctuation(string s)
    {
        int start = 0, end = s.Length - 1;
        while (start <= end && char.IsPunctuation(s[start])) start++;
        while (end >= start && char.IsPunctuation(s[end])) end--;
        return (start > end) ? "" : s[start..(end + 1)];
    }

    private static string NormalizeToken(string s)
        => s.Normalize(NormalizationForm.FormKC)
            .Trim()
            .ToLowerInvariant()
            .Replace(".", "")
            .Replace(",", "");

    private static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);
}
