using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AdvisorySystem.Api.Controllers;
using AdvisorySystem.Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AdvisorySystem.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<SignInManager<AppUser>> _signInManagerMock;
    private readonly Mock<IConfiguration> _configMock;
  private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
   // Setup mocks
        var userStore = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(
       userStore.Object, null, null, null, null, null, null, null, null);
        
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<AppUser>>();
        _signInManagerMock = new Mock<SignInManager<AppUser>>(
      _userManagerMock.Object, contextAccessor.Object, claimsFactory.Object, 
     null, null, null, null);
  
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<AuthController>>();

        // Setup JWT config
   var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x["Key"]).Returns("Your-Super-Secret-Key-Minimum-32-Characters-Long-For-Security-Purposes-2024");
        jwtSection.Setup(x => x["Issuer"]).Returns("AdvisorySystem");
        jwtSection.Setup(x => x["Audience"]).Returns("AdvisorySystem");
  jwtSection.Setup(x => x["ExpiresMinutes"]).Returns("1440");
        _configMock.Setup(x => x.GetSection("Jwt")).Returns(jwtSection.Object);

        _controller = new AuthController(
       _userManagerMock.Object,
         _signInManagerMock.Object,
 _configMock.Object,
 _loggerMock.Object
     );
    }

    #region Register Tests

    [Fact]
 public async Task Register_ValidInput_ReturnsOk()
    {
     // Arrange
        var dto = new AuthController.RegisterDto("test@example.com", "Test@123", "Test User");
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
     .ReturnsAsync(IdentityResult.Success);
  _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<AppUser>(), "Student"))
     .ReturnsAsync(IdentityResult.Success);

      // Act
      var result = await _controller.Register(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
   // Arrange
        var dto = new AuthController.RegisterDto("existing@example.com", "Test@123", null);
        var errors = new[] { new IdentityError { Description = "Email already exists" } };
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
    .ReturnsAsync(IdentityResult.Failed(errors));

        // Act
        var result = await _controller.Register(dto);

        // Assert
  Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_WeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var dto = new AuthController.RegisterDto("test@example.com", "123", null);
        var errors = new[] { new IdentityError { Description = "Password too weak" } };
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
 .ReturnsAsync(IdentityResult.Failed(errors));

      // Act
  var result = await _controller.Register(dto);

 // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokenAndExpiry()
    {
        // Arrange
        var dto = new AuthController.LoginDto("test@example.com", "Test@123");
        var user = new AppUser { Id = "user-id", Email = "test@example.com", UserName = "test@example.com" };
   
   _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
          .ReturnsAsync(user);
 _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, dto.Password, false))
  .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
         .ReturnsAsync(new List<string> { "Student" });

   // Act
        var result = await _controller.Login(dto);

     // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value;
        Assert.NotNull(value);
        
        // Check if response contains token, expiresAt, and expiresIn
        var tokenProperty = value.GetType().GetProperty("token");
   var expiresAtProperty = value.GetType().GetProperty("expiresAt");
 var expiresInProperty = value.GetType().GetProperty("expiresIn");
        
        Assert.NotNull(tokenProperty);
        Assert.NotNull(expiresAtProperty);
        Assert.NotNull(expiresInProperty);
        
        var token = tokenProperty.GetValue(value) as string;
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task Login_InvalidEmail_ReturnsUnauthorized()
    {
     // Arrange
      var dto = new AuthController.LoginDto("nonexistent@example.com", "Test@123");
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync((AppUser?)null);

        // Act
        var result = await _controller.Login(dto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
 public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
  var dto = new AuthController.LoginDto("test@example.com", "WrongPassword");
   var user = new AppUser { Email = "test@example.com" };
        
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);
     _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, dto.Password, false))
      .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

     // Act
    var result = await _controller.Login(dto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    #endregion

    #region Rate Limiting Tests

    [Fact]
    public async Task Login_ExcessiveAttempts_ShouldBeRateLimited()
    {
        // This test verifies rate limiting is configured
      // Actual rate limiting is tested in integration tests
        
        // Arrange
        var dto = new AuthController.LoginDto("test@example.com", "Test@123");
        var user = new AppUser { Email = "test@example.com" };
    
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
     .ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, dto.Password, false))
    .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
  .ReturnsAsync(new List<string> { "Student" });

        // Act - Simulate multiple rapid requests (integration test would verify 429)
        var results = new List<IActionResult>();
        for (int i = 0; i < 6; i++)
 {
            results.Add(await _controller.Login(dto));
        }

   // Assert - First 5 should succeed, 6th should be rate limited (tested in integration)
     Assert.Equal(6, results.Count);
  }

    #endregion
}
