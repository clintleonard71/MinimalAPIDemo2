using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_CouponAPI.Models.DTO
{
    public class CouponRequest
    {
        public string CouponName { get; set; } = string.Empty;

        [FromHeader(Name = "PageSize")]
        public int PageSize { get; set; } = 1;

        [FromHeader(Name = "Page")]
        public int Page { get; set; } = 1;

        public ILogger<CouponRequest> Logger { get; set; } 
    }
}
