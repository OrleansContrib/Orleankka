using System;

using Orleans;

namespace Demo
{
    [Serializable, GenerateSerializer]
    public class HttpException : Exception
    {
        public int StatusCode { get; }

        public HttpException(int statusCode, string message = null)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}