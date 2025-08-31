using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.SharedKernel.Application.Result
{
    public class DataResult<T> : Result, IDataResult<T>
        where T : class, new()
    {
        public T Data { get;}

        public DataResult(T data, bool success, string message) : base(success, message)
        {
            Data = data;
        }

        public DataResult(T data, bool success) : base(success)
        {
            Data = data;
        }
        
    }
}
