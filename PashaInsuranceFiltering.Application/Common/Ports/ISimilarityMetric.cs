namespace PashaInsuranceFiltering.Application.Common.Ports
{
    public interface ISimilarityMetric 
    {
        double Similarity(string a, string b);
    }
}
