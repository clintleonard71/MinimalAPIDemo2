using MagicVilla_CouponAPI.Models.DTO;

namespace MagicVilla_CouponAPI.Repository
{
    public interface IAuthRepository
    {
        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);
        Task<UserDTO> Register(RegistrationRequestDTO registrationRequestDTO);
        Task<bool> IsUniqueUser(string userName);
    }
}
