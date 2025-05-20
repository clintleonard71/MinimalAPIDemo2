using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_CouponAPI.Repository
{
    public class CouponRepository : ICouponRepository
    {
        private readonly ApplicationDBContext _db;
        public CouponRepository(ApplicationDBContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Coupon> CreateCouponAsync(Coupon coupon)
        {
            if (coupon == null)
            {
                throw new ArgumentNullException(nameof(coupon));
            }

            await _db.Coupons.AddAsync(coupon);
            await _db.SaveChangesAsync();
            return coupon;
        }

        public async Task<bool> DeleteCouponAsync(Coupon coupon)
        {            
            if (coupon == null)
                return false;
         
            _db.Coupons.Remove(coupon);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<Coupon> GetCouponByIdAsync(int id)
        {
            // Use 'await' to asynchronously fetch the coupon from the database
            var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Id == id);

            // Ensure all code paths return a value
            return coupon;
        }

        public async Task<Coupon> GetCouponByNameAsync(string couponName)
        {
            if (string.IsNullOrWhiteSpace(couponName))
            {
                throw new ArgumentException("Coupon name cannot be null or empty.", nameof(couponName));
            }

            // Use 'await' to asynchronously fetch the coupon from the database
            var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Name.ToLower() == couponName.ToLower());

            // Ensure all code paths return a value
            return coupon;
        }

        public async Task<IEnumerable<Coupon>> GetCouponsAsync()
        {
            return await _db.Coupons.ToListAsync();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<Coupon> UpdateCouponAsync(Coupon coupon)
        {
            if (coupon == null)
            {
                throw new ArgumentNullException(nameof(coupon));
            }

            var existingCoupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Id == coupon.Id);
            if (existingCoupon == null)
            {
                throw new InvalidOperationException("Coupon not found.");
            }

            existingCoupon.Name = coupon.Name;
            existingCoupon.Percent = coupon.Percent;
            existingCoupon.IsActive = coupon.IsActive;
            existingCoupon.Updated = DateTime.UtcNow;

            _db.Coupons.Update(existingCoupon);
            await _db.SaveChangesAsync();

            return existingCoupon;
        }
    }
}
