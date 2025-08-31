using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.SharedKernel.Application.Result
{
    public class Result(bool success) : IResult
    {
        public bool Success { get; set; } = success;
        public string Message { get; set; } = string.Empty;

        public Result(bool success, string message) : this(success)
        {
            Message = message;
        }
        
    }
}
