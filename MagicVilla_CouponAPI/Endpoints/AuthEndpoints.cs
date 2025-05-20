using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_CouponAPI.AuthEndpoints
{
    public static class AuthEndpoints
    {
        public static void ConfigureAuthEndpoints(this WebApplication app)
        {    
            app.MapPost("/api/login/", LoginAsync).WithName("Login").Accepts<LoginRequestDTO>("application/json")
                  .Produces<ApiResponse>(200).Produces<ApiResponse>(400);

            app.MapPost("/api/register/", RegisterAsync).WithName("Register").Accepts<LoginRequestDTO>("application/json")
                  .Produces<ApiResponse>(200).Produces<ApiResponse>(400);

        }

        private static async Task<IResult> LoginAsync([FromServices] IAuthRepository _authRepo,
                                                      [FromServices] IMapper _mapper,                                                     
                                                      [FromBody] LoginRequestDTO loginRequestDTO)
        {
            ApiResponse response = new ApiResponse() { isSuccess = false, StatusCode = HttpStatusCode.BadRequest };

            var loginResponseDTO = await _authRepo.Login(loginRequestDTO);

            if (loginResponseDTO == null)
            {
                response.Errors.Add("User and/or password is incorrect");
                return Results.BadRequest(response);
            }

            response.isSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = loginResponseDTO;
            return Results.Ok(response);
        }

        private static async Task<IResult> RegisterAsync([FromServices] IAuthRepository _authRepo,
                                                         [FromServices] IMapper _mapper,
                                                         [FromBody] RegistrationRequestDTO registrationRequestDTO)
        {
            ApiResponse response = new ApiResponse() { isSuccess = false, StatusCode = HttpStatusCode.BadRequest };

            bool ifUserNameIsUnique = await _authRepo.IsUniqueUser(registrationRequestDTO.UserName);
            if (!ifUserNameIsUnique)
            {
                response.Errors.Add("User name already exists");
                return Results.BadRequest(response);
            }

            var registerResponse = await _authRepo.Register(registrationRequestDTO);

            if (registerResponse == null || string.IsNullOrEmpty(registerResponse.UserName))
            {
                response.Errors.Add("registration failed");
                return Results.BadRequest(response);
            }
           
            response.isSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = registerResponse;
            return Results.Ok(response);
        }

        private static string GetDebuggerDisplay(this Program program) => $"{program.GetType().Name}";


    }
}
