using System.Net;

namespace MagicVilla_CouponAPI.Models
{
    public class ApiResponse
    {
        public ApiResponse()
        {
            
        }
        public ApiResponse(object result, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Result = result;
            StatusCode = statusCode;
        }

        public bool isSuccess { get; set; } = true;
        public object Result { get; set; }
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public List<string> Errors { get; set; } = new List<string>();
    }
}
