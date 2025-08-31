using FluentAssertions;
using PashaInsuranceFiltering.Infrastructure.Filtering;

namespace PashaInsuranceFiltering.Tests.Unit.Filtering
{
    public class JaroWinklerMetricTests
    {
        [Theory]
        [InlineData("password", "pasword", 0.90)]
        [InlineData("card", "cart", 0.80)]
        [InlineData("secret", "secreter", 0.80)]
        [InlineData("confidential", "confidantial", 0.90)]
        [InlineData("apple", "apply", 0.85)]
        public void SimilarityShouldBeHighForKnownClosePairs(string a, string b, double minExpected)
        {
            var jw = new JaroWinklerMetric();
            var similarity = jw.Similarity(a, b);

            similarity.Should().BeGreaterThanOrEqualTo(minExpected);
            similarity.Should().BeInRange(0.0, 1.0);
        }

        [Fact]
        public void SimilarityShouldBeOneForIdenticalStringsCaseInsensitive()
        {
            var jw = new JaroWinklerMetric();
            jw.Similarity("Password", "password").Should().Be(1.0);
            jw.Similarity("SECRET", "secret").Should().Be(1.0);
        }

        [Fact]
        public void SimilarityShouldBeSymmetric()
        {
            var jw = new JaroWinklerMetric();
            var ab = jw.Similarity("black", "blank");
            var ba = jw.Similarity("blank", "black");

            ab.Should().BeApproximately(ba, 1e-9);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", "abc")]
        [InlineData("abc", "")]
        public void SimilarityHandlesEmptyStringsAndStaysInBounds(string a, string b)
        {
            var jw = new JaroWinklerMetric();
            var s = jw.Similarity(a, b);

            s.Should().BeInRange(0.0, 1.0);
        }


    }
}
