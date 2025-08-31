using FluentAssertions;
using PashaInsuranceFiltering.Infrastructure.Filtering;
using Xunit;

namespace PashaInsuranceFiltering.Tests.Unit.Filtering
{
    public class LevenshteinMetricTests
    {
        [Theory]
        [InlineData("saturday", "sunday", 0.60)]
        [InlineData("flaw", "lawn", 0.50)]
        [InlineData("gumbo", "gambol", 0.60)]
        public void SimilarityShouldBeReasonablyHighForRelatedWords(string a, string b, double minExpected)
        {
            var lev = new LevenshteinMetric();
            var s = lev.Similarity(a, b);

            s.Should().BeInRange(0.0, 1.0);
            s.Should().BeGreaterThanOrEqualTo(minExpected);
        }

        [Fact]
        public void SimilarityShouldBeOneForIdenticalStringsCaseInsensitive()
        {
            var lev = new LevenshteinMetric();

            lev.Similarity("Levenshtein", "levenshtein").Should().Be(1.0);
            lev.Similarity("API", "api").Should().Be(1.0);
        }

        [Fact]
        public void SimilarityShouldBeZeroWhenOneSideIsEmptyAndOtherIsNot()
        {
            var lev = new LevenshteinMetric();

            lev.Similarity("", "abc").Should().Be(0.0);
            lev.Similarity("abc", "").Should().Be(0.0);
        }

        [Fact]
        public void SimilarityShouldBeOneWhenBothAreEmptyOrWhitespaceEqual()
        {
            var lev = new LevenshteinMetric();

            lev.Similarity("", "").Should().Be(1.0);
            lev.Similarity("   ", "   ").Should().Be(1.0);
        }

        [Fact]
        public void SimilarityShouldBeSymmetric()
        {
            var lev = new LevenshteinMetric();

            var ab = lev.Similarity("distance", "instance");
            var ba = lev.Similarity("instance", "distance");

            ab.Should().BeApproximately(ba, 1e-9);
        }

        [Fact]
        public void SimilarityShouldIncreaseWhenStringsGetCloser()
        {
            var lev = new LevenshteinMetric();

            var far = lev.Similarity("abcdef", "uvwxyz");
            var close = lev.Similarity("abcdef", "abcxef");

            close.Should().BeGreaterThan(far);
        }
    }

}

