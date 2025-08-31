using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.Application.Common.Ports
{
    public interface ITextFilter
    {
        Task<string> FilterAsync(string input, double threshold, CancellationToken ct = default);
    }
}
