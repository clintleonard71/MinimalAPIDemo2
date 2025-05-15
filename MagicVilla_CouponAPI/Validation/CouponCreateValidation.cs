using FluentValidation;

namespace MagicVilla_CouponAPI.Validation
{
    public class CouponCreateValidation : AbstractValidator<Models.DTO.CouponCreateDTO>
    {
        public CouponCreateValidation()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Coupon name is required.")
                .MinimumLength(3)
                .WithMessage("Coupon name must be at least 3 characters long.");
            RuleFor(x => x.Percent)
                .NotEmpty()
                .WithMessage("Coupon percent is required.")
                .InclusiveBetween(1, 100)
                .WithMessage("Coupon percent must be between 1 and 100.");
            RuleFor(x => x.IsActive)
                .NotNull()
                .WithMessage("IsActive status is required.");
        }
    }        
}
