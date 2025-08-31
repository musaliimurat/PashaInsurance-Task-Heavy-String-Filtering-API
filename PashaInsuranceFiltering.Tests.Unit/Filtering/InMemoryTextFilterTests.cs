using FluentAssertions;
using PashaInsuranceFiltering.Infrastructure.Filtering;

namespace PashaInsuranceFiltering.Tests.Unit.Filtering
{
    public class InMemoryTextFilterTests
    {
        private readonly JaroWinklerMetric _metric = new();

        [Fact]
        public void FilterShouldRemoveBannedWordsAndPhrases()
        {
            var bannedWords = new[] 
            {
                   // EN – secrets/sensitive
               "password",
               "passphrase",
               "secret",
               "token",
               "api key",
               "api_key",
               "access key",
               "client secret",
               "client_secret",
               "connection string",
               "jwt",
               "bearer",
               "refresh token",
               "private key",
               "ssh key",
               "confidential",
               "internal use only",
               "do not distribute",
               // EN – PII & financial
               "ssn",
               "social security",
               "credit card",
               "card number",
               "cvv",
               "iban",
               "swift",
               "routing number",
               // EN – company-sensitive
               "roadmap",
               "nda",
               "legal hold",
               "privileged",
               // AZ
               "şifrə",
               "gizli",
               "məxfi",
               "paylaşma",
               "daxili istifadə",
               "müştəri sirri",
               "hesab nömrəsi",
               "kart nömrəsi",
               "sirri"
            };

            var filter = new InMemoryTextFilter(bannedWords, _metric);
            var input = "This is a secret document. Do not share your password or api key. " +
                "It contains confidential information for internal use only. " +
                "bu məqalədə interface-nin ilkin anlayışlarını sizə izah etməyə çalışacam. Təbii ki, interface bununla bitmir və daha irəli səviyyəsi mövcüddur. " +
                "Növbəti məqalələrdə izah edəcəm My credit card number is 1234-5678-9012-3456.";

            var output = filter.Filter(input, 0.85);


            foreach (var word in bannedWords)
                output.Should().NotContainEquivalentOf(word);
            
        }
    }
}
