
using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;

namespace MagicVilla_CouponAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddAutoMapper(typeof(MappingConfig));

            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                
                app.MapScalarApiReference(options =>
                {
                    options.Title = "Coupon API";
                    options.Layout = ScalarLayout.Modern; // Set default layout
                    options.ShowSidebar = true; // Ensure sidebar is visible
                    options.WithForceThemeMode(ThemeMode.Dark); // Default to dark mode if preferred
                    options.WithOpenApiRoutePattern("/openapi/v1.json");
                });

                app.UseDefaultFiles(); // Ensures Scalar is the default page
                app.UseStaticFiles();
            }
                    
            app.MapGet("/api/coupon/", (ILogger<Program> _logger) => 
            {                
                _logger.Log(LogLevel.Information, "Get all coupons called");

                ApiResponse apiResponse = new ApiResponse(CouponStore.Coupons, HttpStatusCode.OK);
                
                return Results.Ok(apiResponse);

            }).WithName("GetCoupons")
              .Produces<ApiResponse>(200);

            app.MapGet("/api/coupon/{id:int}", (int id) => 
            {
                ApiResponse apiResponse = 
                    new ApiResponse(CouponStore.Coupons.FirstOrDefault(c => c.Id == id), HttpStatusCode.OK);

                return apiResponse;

            }).WithName("GetCoupon").Produces<ApiResponse>(200);


            //POST /api/coupon            
            app.MapPost("/api/coupon/",                 
                async (HttpContext context) => 
                {
                    // Resolve dependencies from DI container
                    var _validator = context.RequestServices.GetRequiredService<IValidator<CouponCreateDTO>>();
                    var _mapper = context.RequestServices.GetRequiredService<IMapper>();
                    var couponCreateDto = await context.Request.ReadFromJsonAsync<CouponCreateDTO>();

                    var validationResult = await _validator.ValidateAsync(couponCreateDto);
                    if (!validationResult.IsValid)
                    {
                        return Results.BadRequest(validationResult.Errors); 
                    }

                    Coupon coupon = _mapper.Map<Coupon>(couponCreateDto);
                    
                    coupon.Id = CouponStore.Coupons.Max(c => c.Id) + 1;

                    CouponStore.Coupons.Add(coupon);

                    CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);

                    ApiResponse apiResponse = new ApiResponse(couponDTO, HttpStatusCode.OK);

                    return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, couponDTO);
                    //return Results.Created($"/api/coupon/{coupon.Id}", coupon);

                }).WithName("CreateCoupon")
                  .Accepts<CouponCreateDTO>("application/json")
                  .Produces<CouponDTO>(200)
                  .Produces(400);

            //PUT /api/coupon/
            app.MapPut("/api/coupon/",
                async ([FromBody] CouponUpdateDTO couponUpdateDto, HttpContext context) =>
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
                        return Task.FromResult(apiResponse);
                    }

                    if (!CouponStore.Coupons.Exists(c => c.Id == couponUpdateDto.Id))
                    {
                        apiResponse.Errors.Add("Coupon not found");
                        apiResponse.StatusCode = HttpStatusCode.NotFound;
                        return Task.FromResult(apiResponse);
                    }

                    Coupon coupon = _mapper.Map<Coupon>(couponUpdateDto);
                    CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);

                    CouponStore.Coupons[coupon.Id - 1] = coupon;
                     
                    apiResponse.Result = couponDTO;

                    return Task.FromResult(apiResponse);
                    //return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, couponDTO);
                    //return Results.Created($"/api/coupon/{coupon.Id}", coupon);

                }).WithName("UpdateCoupon")
                  .Accepts<CouponUpdateDTO>("application/json")
                  .Produces<ApiResponse>(200)
                  .Produces<ApiResponse>(400)
                  .Produces<ApiResponse>(404);
                  

            app.MapDelete("/api/coupon/{id:int}", (int id) => {

                ApiResponse apiResponse = new ApiResponse(id, HttpStatusCode.OK);
                if (!CouponStore.Coupons.Exists(c => c.Id == id))
                {
                    apiResponse.Errors.Add($"Coupon {id} not found");
                    apiResponse.StatusCode = HttpStatusCode.NotFound;
                    return apiResponse;
                }
                
                var couponStore = CouponStore.Coupons.Where(c => c.Id != id).ToList();

                CouponStore.Coupons = couponStore;

                apiResponse.Result = $"Coupon deleted. Id deleted = {id}.  Num coupons remaining is {CouponStore.Coupons.Count()}";
                apiResponse.StatusCode = HttpStatusCode.OK;
                
                return apiResponse;

            }).WithName("DeleteCoupon");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.Run();
            
        }
    }
}

