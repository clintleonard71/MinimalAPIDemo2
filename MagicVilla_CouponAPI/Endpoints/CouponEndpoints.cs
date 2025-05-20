using AutoMapper;
using Azure.Core;
using FluentValidation;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_CouponAPI.CouponEndpoints
{
    public static class CouponEndpoints
    {
        public  static void ConfigureCouponEndpoints(this WebApplication app)
        {

            #region API CALLS

            //GET /api/coupon        
            app.MapGet("/api/coupon/", GetCouponsAsync).WithName("GetCoupons").Produces<ApiResponse>(200).RequireAuthorization("AdminOnly");

            //GET /api/coupon        
            app.MapGet("/api/coupon/special", GetCouponsSpecialAsync).WithName("GetCouponsSpecial").Produces<ApiResponse>(200);

            // GET /api/coupon/{id:int}
            app.MapGet("/api/coupon/{id:int}", GetCouponByIdAsync).WithName("GetCoupon").Produces<ApiResponse>(200)
                .AddEndpointFilter(async (context, next) =>
                {
                    // Correctly retrieve the route value using the HttpContext from the context
                    var id = int.Parse(context.HttpContext.Request.RouteValues["id"]?.ToString() ?? "0");
                    if (id == 0)
                    {
                        return Results.BadRequest(new ApiResponse($"Invalid ID {id}", HttpStatusCode.BadRequest));
                    }

                    Console.WriteLine($"Before 1st filter ID: {id}");

                    var result = await next(context);

                    Console.WriteLine($"After 1st filter ID: {id}");

                    return result;
                })
                .AddEndpointFilter(async (context, next) =>
                {
                    Console.WriteLine($"Before 2nd filter");

                    var result = await next(context);

                    Console.WriteLine($"After 2nd filter");

                    return result;
                });

            //POST /api/coupon            
            app.MapPost("/api/coupon/", CreateCouponAsync).WithName("GetCoupon").WithName("CreateCoupon").Accepts<CouponCreateDTO>("application/json")
                  .Produces<ApiResponse>(200).Produces<ApiResponse>(400);

            //PUT /api/coupon/
            app.MapPut("/api/coupon/", UpdateCouponAsync).WithName("UpdateCoupon").Accepts<CouponUpdateDTO>("application/json")
                  .Produces<ApiResponse>(200).Produces<ApiResponse>(400).Produces<ApiResponse>(404);

            //DELETE /api/coupon/{id:int}
            app.MapDelete("/api/coupon/{id:int}", DeleteCouponAsync).WithName("DeleteCoupon").Produces<ApiResponse>(200).Produces<ApiResponse>(404);

            #endregion API CALLS
        }

        private async static Task<IResult> GetCouponsAsync(ICouponRepository _couponRepo, ILogger<Program> _logger)
        {
            _logger.Log(LogLevel.Information, "Get all coupons called");
            ApiResponse apiResponse = new ApiResponse(await _couponRepo.GetCouponsAsync(), HttpStatusCode.OK);
            apiResponse.isSuccess = true;
            return Results.Ok(apiResponse);
        }

        private async static Task<IResult> GetCouponsSpecialAsync([AsParameters] CouponRequest request)
        {
            request.Logger.Log(LogLevel.Information, "Get all coupons special called with parms");
            
            // Simulate asynchronous behavior to resolve CS1998
            await Task.CompletedTask;

            ApiResponse apiResponse = new ApiResponse($"couponName {request.CouponName}, PageSize {request.PageSize}, Page {request.Page}", HttpStatusCode.OK);
            apiResponse.isSuccess = true;
            return Results.Ok(apiResponse);
        }

        private async static Task<IResult> GetCouponByIdAsync(ICouponRepository _couponRepo, ILogger<Program> _logger, int id)
        {
            _logger.Log(LogLevel.Information, "GetCouponByIdAsync Begin");

            ApiResponse apiResponse = new ApiResponse(await _couponRepo.GetCouponByIdAsync(id), HttpStatusCode.OK);
            apiResponse.isSuccess = true;

            _logger.Log(LogLevel.Information, "GetCouponByIdAsync End");


            return Results.Ok(apiResponse);
        }

        private async static Task<IResult> CreateCouponAsync(ICouponRepository _couponRepo, HttpContext context)
        {
            ApiResponse response = new ApiResponse() { isSuccess = false, StatusCode = HttpStatusCode.BadRequest };
            // Resolve dependencies from DI container
            var _validator = context.RequestServices.GetRequiredService<IValidator<CouponCreateDTO>>();
            var _mapper = context.RequestServices.GetRequiredService<IMapper>();
            var couponCreateDto = await context.Request.ReadFromJsonAsync<CouponCreateDTO>();
            var validationResult = await _validator.ValidateAsync(couponCreateDto);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }
            if (await _couponRepo.GetCouponByNameAsync(couponCreateDto.Name.ToLower()) != null)
            {
                response.Errors.Add("Coupon name already exists");
                return Results.BadRequest(response);
            }
            Coupon newCoupon = _mapper.Map<Coupon>(couponCreateDto);
            await _couponRepo.CreateCouponAsync(newCoupon);
            CouponDTO newCouponDTO = _mapper.Map<CouponDTO>(newCoupon);
            response.isSuccess = true;
            response.Result = newCouponDTO;
            return Results.Ok(response);
        }

        private async static Task<IResult> UpdateCouponAsync(ICouponRepository _couponRepo, [FromBody] CouponUpdateDTO couponUpdateDto, HttpContext context)
        {
            // Resolve dependencies from DI container
            var _validator = context.RequestServices.GetRequiredService<IValidator<CouponUpdateDTO>>();
            var _mapper = context.RequestServices.GetRequiredService<IMapper>();
            ApiResponse apiResponse = new ApiResponse(couponUpdateDto, HttpStatusCode.OK);
            var validationResult = await _validator.ValidateAsync(couponUpdateDto);
            if (!validationResult.IsValid)
            {
                apiResponse.Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                apiResponse.StatusCode = HttpStatusCode.BadRequest;
                return Results.BadRequest(apiResponse);
            }
            await _couponRepo.UpdateCouponAsync(_mapper.Map<Coupon>(couponUpdateDto));
            apiResponse.Result = _mapper.Map<Coupon>(await _couponRepo.GetCouponByIdAsync(couponUpdateDto.Id));
            apiResponse.isSuccess = true;
            apiResponse.StatusCode = HttpStatusCode.OK;
            return Results.Ok(apiResponse);
        }

        private async static Task<IResult> DeleteCouponAsync(ICouponRepository _couponRepo, int id)
        {
            ApiResponse apiResponse = new ApiResponse(id, HttpStatusCode.OK);
            Coupon couponToDelete = await _couponRepo.GetCouponByIdAsync(id);
            if (couponToDelete == null)
            {
                apiResponse.Errors.Add($"Coupon {id} not found");
                apiResponse.StatusCode = HttpStatusCode.NotFound;
                return Results.NotFound(apiResponse);
            }

            await _couponRepo.DeleteCouponAsync(couponToDelete);
            apiResponse.Result = $"Coupon deleted. Id deleted = {id}";
            apiResponse.StatusCode = HttpStatusCode.OK;
            return Results.Ok(apiResponse);
        }

        private static string GetDebuggerDisplay(this Program program) => $"{program.GetType().Name}";


    }
}
