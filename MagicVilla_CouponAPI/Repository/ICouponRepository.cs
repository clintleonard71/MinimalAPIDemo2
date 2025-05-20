using MagicVilla_CouponAPI.Models;

namespace MagicVilla_CouponAPI.Repository
{
    public interface ICouponRepository
    {
        Task<IEnumerable<Coupon>> GetCouponsAsync();
        Task<Coupon> GetCouponByIdAsync(int id);
        Task<Coupon> GetCouponByNameAsync(string couponName);
        Task<Coupon> CreateCouponAsync(Coupon coupon);
        Task<Coupon> UpdateCouponAsync(Coupon coupon);
        Task<bool> DeleteCouponAsync(Coupon coupon);

        Task SaveAsync();
    }
}

