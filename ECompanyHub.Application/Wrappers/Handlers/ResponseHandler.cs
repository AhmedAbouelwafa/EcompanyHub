using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECompanyHub.Application.Wrappers.Handlers
{
    public class ResponseHandler<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; }

        public static ResponseHandler<T> SuccessResponse(T data)
        {
            return new ResponseHandler<T>
            {
                Success = true,
                Data = data,
                Errors = null
            };
        }

        public static ResponseHandler<T> FailureResponse(params string[] errors)
        {
            return new ResponseHandler<T>
            {
                Success = false,
                Data = default,
                Errors = errors.ToList()
            };
        }
    }



}
