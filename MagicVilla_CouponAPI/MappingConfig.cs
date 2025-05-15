using AutoMapper;

namespace MagicVilla_CouponAPI
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<Models.Coupon, Models.DTO.CouponDTO>().ReverseMap();
            CreateMap<Models.Coupon, Models.DTO.CouponCreateDTO>().ReverseMap();
            CreateMap<Models.Coupon, Models.DTO.CouponUpdateDTO>().ReverseMap();
            //CreateMap<Models.Coupon, Models.DTO.CouponUpdateDTO>().ReverseMap();
        }
    }
}
