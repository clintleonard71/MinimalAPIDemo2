using Xunit;
using Moq; // Ensure this using directive is present
using AutoMapper;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Repository;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Threading.Tasks;
using MagicVilla_CouponAPI.AuthEndpoints;

public class AuthEndpointsTests
{
    [Fact]
    public async Task RegisterAsync_ReturnsBadRequest_WhenUserNameIsNotUnique()
    {
        // Arrange
        var mockRepo = new Mock<IAuthRepository>();
        var mockMapper = new Mock<IMapper>();
        var registrationDto = new RegistrationRequestDTO { UserName = "testuser" };

        mockRepo.Setup(r => r.IsUniqueUser("testuser")).ReturnsAsync(false);

        // Act
        var result = await InvokeRegisterAsync(mockRepo.Object, mockMapper.Object, registrationDto);

        // Assert
        var badRequest = Assert.IsType<BadRequest<ApiResponse>>(result);
        var response = Assert.IsType<ApiResponse>(badRequest.Value);
        Assert.Contains("User name already exists", response.Errors);
        Assert.False(response.isSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsBadRequest_WhenRegistrationFails()
    {
        // Arrange
        var mockRepo = new Mock<IAuthRepository>();
        var mockMapper = new Mock<IMapper>();
        var registrationDto = new RegistrationRequestDTO { UserName = "testuser" };

        mockRepo.Setup(r => r.IsUniqueUser("testuser")).ReturnsAsync(true);
        mockRepo.Setup(r => r.Register(registrationDto)).ReturnsAsync((UserDTO)null);

        // Act
        var result = await InvokeRegisterAsync(mockRepo.Object, mockMapper.Object, registrationDto);

        // Assert
        var badRequest = Assert.IsType<BadRequest<ApiResponse>>(result);
        var response = Assert.IsType<ApiResponse>(badRequest.Value);
        Assert.Contains("registration failed", response.Errors);
        Assert.False(response.isSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsOk_WhenRegistrationSucceeds()
    {
        // Arrange
        var mockRepo = new Mock<IAuthRepository>();
        var mockMapper = new Mock<IMapper>();
        var registrationDto = new RegistrationRequestDTO { UserName = "testuser" };
        var userDto = new UserDTO { UserName = "testuser" };

        mockRepo.Setup(r => r.IsUniqueUser("testuser")).ReturnsAsync(true);
        mockRepo.Setup(r => r.Register(registrationDto)).ReturnsAsync(userDto);

        // Act
        var result = await InvokeRegisterAsync(mockRepo.Object, mockMapper.Object, registrationDto);

        // Assert
        var okResult = Assert.IsType<Ok<ApiResponse>>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.isSuccess);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(userDto, response.Result);
    }

    [Fact]
    public async Task LoginAsync_ReturnsBadRequest_WhenLoginFails()
    {
        // Arrange
        var mockRepo = new Mock<IAuthRepository>();
        var mockMapper = new Mock<IMapper>();
        var loginDto = new LoginRequestDTO { UserName = "testuser", Password = "pass" };

        mockRepo.Setup(r => r.Login(loginDto)).ReturnsAsync((LoginResponseDTO)null);

        // Act
        var result = await InvokeLoginAsync(mockRepo.Object, mockMapper.Object, loginDto);

        // Assert
        var badRequest = Assert.IsType<BadRequest<ApiResponse>>(result);
        var response = Assert.IsType<ApiResponse>(badRequest.Value);
        Assert.Contains("User and/or password is incorrect", response.Errors);
        Assert.False(response.isSuccess);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LoginAsync_ReturnsOk_WhenLoginSucceeds()
    {
        // Arrange
        var mockRepo = new Mock<IAuthRepository>();
        var mockMapper = new Mock<IMapper>();
        var loginDto = new LoginRequestDTO { UserName = "testuser", Password = "pass" };
        var loginResponse = new LoginResponseDTO { User = new UserDTO { UserName = "testuser" }, Token = "token" };

        mockRepo.Setup(r => r.Login(loginDto)).ReturnsAsync(loginResponse);

        // Act
        var result = await InvokeLoginAsync(mockRepo.Object, mockMapper.Object, loginDto);

        // Assert
        var okResult = Assert.IsType<Ok<ApiResponse>>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.isSuccess);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(loginResponse, response.Result);
    }

    // Helper methods to invoke private static endpoint methods using reflection
    private static Task<IResult> InvokeRegisterAsync(IAuthRepository repo, IMapper mapper, RegistrationRequestDTO dto)
    {
        var method = typeof(AuthEndpoints).GetMethod("RegisterAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        return (Task<IResult>)method.Invoke(null, new object[] { repo, mapper, dto });
    }

    private static Task<IResult> InvokeLoginAsync(IAuthRepository repo, IMapper mapper, LoginRequestDTO dto)
    {
        var method = typeof(AuthEndpoints).GetMethod("LoginAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        return (Task<IResult>)method.Invoke(null, new object[] { repo, mapper, dto });
    }
}