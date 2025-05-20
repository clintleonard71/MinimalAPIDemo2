using FluentValidation;
using MagicVilla_CouponAPI.AuthEndpoints;
using MagicVilla_CouponAPI.CouponEndpoints;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace MagicVilla_CouponAPI
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddOpenApi("v1", options =>
            {
                options.AddDocumentTransformer(async (document, context, cancellationToken) =>
                {
                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        In = ParameterLocation.Header,
                        BearerFormat = "JWT",
                        Description = "JWT Authorization header using the Bearer scheme."
                    };

                    // Optionally add security requirements to each operation:
                    foreach (var path in document.Paths.Values)
                    {
                        foreach (var operation in path.Operations.Values)
                        {
                            operation.Security.Add(new OpenApiSecurityRequirement
                            {
                                        {
                                            new OpenApiSecurityScheme
                                            {
                                                Reference = new OpenApiReference
                                                {
                                                    Type = ReferenceType.SecurityScheme,
                                                    Id = "Bearer"
                                                }
                                            },
                                            Array.Empty<string>()
                                        }
                            });
                        }
                    }

                    await Task.CompletedTask; // Ensure the lambda returns a Task
                });
            });

            //builder.Services.AddSingleton<IAuthenticationSchemeProvider, >();
            builder.Services.AddScoped<ICouponRepository, CouponRepository>();
            builder.Services.AddScoped<IAuthRepository, AuthRepository>();

            builder.Services.AddAutoMapper(typeof(MappingConfig));

            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            builder.Services.AddDbContext<ApplicationDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
                );

            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {

                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["ApiSettings:Secret"]!)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            builder.Services.AddAuthorization(options => 
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
            });


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

                    options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.AsyncHttp);

                    options.AddHttpAuthentication("Bearer", scheme =>
                    {
                        scheme.Description = "Bearer token for authentication";
                    });

                    options.Authentication = new ScalarAuthenticationOptions
                    {
                        PreferredSecurityScheme = "Bearer"
                    };
                });

                app.UseDefaultFiles(); // Ensures Scalar is the default page
                app.UseStaticFiles();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.ConfigureAuthEndpoints();
            app.ConfigureCouponEndpoints();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.Run();

        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}

