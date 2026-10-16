using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RaceDay.API.Controllers;
using RaceDay.API.Models.DTOs;
using RaceDay.API.Services;
using RaceDay.Tests.Helpers;
using Xunit;

namespace RaceDay.Tests;

public class AuthControllerTests
{
    private static TokenService CreateTokenService()
    {
        var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestKey_ThisIsAtLeast32CharactersLong_ForHmacSha256!",
                ["Jwt:Issuer"] = "RaceDayAPI",
                ["Jwt:Audience"] = "RaceDayClients",
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();

        return new TokenService(config);
    }

    [Fact]
    public async Task Register_WithValidOrganiser_ReturnsCreated()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var controller = new AuthController(db, CreateTokenService());

        var dto = new RegisterDto(
            Email: "organiser@test.co.za",
            Password: "Password123!",
            FirstName: "John",
            LastName: "Doe",
            Role: "Organiser",
            OrganisationName: "Comrades Marathon Association",
            ContactPhone: "+27123456789",
            Website: "https://comrades.com",
            DateOfBirth: null,
            Gender: null,
            EmergencyContactName: null,
            EmergencyContactPhone: null,
            MedicalConditions: null,
            ClubName: null
        );

        // Act
        var result = await controller.Register(dto);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);

        var response = objectResult.Value.Should().BeOfType<AuthResponseDto>().Subject;
        response.Role.Should().Be("Organiser");
        response.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithValidParticipant_ReturnsCreated()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var controller = new AuthController(db, CreateTokenService());

        var dto = new RegisterDto(
            Email: "participant@test.co.za",
            Password: "Password123!",
            FirstName: "Sarah",
            LastName: "Patel",
            Role: "Participant",
            OrganisationName: null,
            ContactPhone: null,
            Website: null,
            DateOfBirth: new DateTime(1990, 1, 1),
            Gender: "Female",
            EmergencyContactName: "Mr Patel",
            EmergencyContactPhone: "+27821234567",
            MedicalConditions: "None",
            ClubName: "Celtic Harriers"
        );

        // Act
        var result = await controller.Register(dto);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Register_WithInvalidRole_ReturnsBadRequest()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var controller = new AuthController(db, CreateTokenService());

        var dto = new RegisterDto(
            "user@test.co.za", "Password123!", "A", "B", "Admin",
            null, null, null, null, null, null, null, null, null);

        // Act
        var result = await controller.Register(dto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var controller = new AuthController(db, CreateTokenService());

        var dto = new RegisterDto(
            "dup@test.co.za", "Password123!", "A", "B", "Participant",
            null, null, null, DateTime.UtcNow.AddYears(-20), null, "N/A", "N/A", null, null);

        await controller.Register(dto);

        // Act
        var result = await controller.Register(dto);

        // Assert
        result.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsToken()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var controller = new AuthController(db, CreateTokenService());

        await controller.Register(new RegisterDto(
            "login@test.co.za", "Password123!", "A", "B", "Participant",
            null, null, null, DateTime.UtcNow.AddYears(-20), null, "N/A", "N/A", null, null));

        // Act
        var result = await controller.Login(new LoginDto("login@test.co.za", "Password123!"));

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<AuthResponseDto>().Subject;
        response.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var controller = new AuthController(db, CreateTokenService());

        await controller.Register(new RegisterDto(
            "wrong@test.co.za", "Password123!", "A", "B", "Participant",
            null, null, null, DateTime.UtcNow.AddYears(-20), null, "N/A", "N/A", null, null));

        // Act
        var result = await controller.Login(new LoginDto("wrong@test.co.za", "WrongPassword!"));

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        using var db = TestDbContextFactory.Create();
        var controller = new AuthController(db, CreateTokenService());

        var result = await controller.Login(new LoginDto("nobody@test.co.za", "Password123!"));

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}