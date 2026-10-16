using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Data;
using RaceDay.API.Controllers;
using RaceDay.API.Models.DTOs;
using RaceDay.API.Models.Entities;
using RaceDay.Tests.Helpers;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RaceDay.Tests;

public class EnrolmentControllerTests
{
    private static (User organiser, User participant, Category category)
        SeedEventAndCategory(RaceDayDbContext db)
    {
        var organiserUser = new User
        {
            Email = "org@test.co.za",
            PasswordHash = "x",
            FirstName = "Org",
            LastName = "User",
            Role = "Organiser",
            Organiser = new EventOrganiser
            {
                OrganisationName = "Test Org",
                ContactPhone = "+27123456789"
            }
        };
        db.Users.Add(organiserUser);
        db.SaveChanges();

        var participantUser = new User
        {
            Email = "part@test.co.za",
            PasswordHash = "x",
            FirstName = "Part",
            LastName = "User",
            Role = "Participant",
            Participant = new Participant
            {
                DateOfBirth = new DateTime(1990, 1, 1),
                EmergencyContactName = "N/A",
                EmergencyContactPhone = "N/A"
            }
        };
        db.Users.Add(participantUser);
        db.SaveChanges();

        var ev = new Event
        {
            OrganiserId = organiserUser.Organiser!.OrganiserId,
            EventName = "Test Event",
            EventType = "Running",
            DateTime = DateTime.UtcNow.AddMonths(2),
            Location = "PMB",
            MaxParticipants = 100,
            Status = "Open"
        };
        db.Events.Add(ev);
        db.SaveChanges();

        var category = new Category
        {
            EventId = ev.EventId,
            CategoryName = "42.2km",
            DistanceKm = 42.2m,
            GenderRestriction = "Open"
        };
        db.Categories.Add(category);
        db.SaveChanges();

        return (organiserUser, participantUser, category);
    }

    [Fact]
    public async Task Enrol_AsParticipant_ReturnsCreated()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var (_, participant, category) = SeedEventAndCategory(db);

        var controller = new EnrolmentsController(db);
        ControllerTestHelper.SetUser(controller, participant);

        var dto = new CreateEnrolmentDto(category.CategoryId, "PAY-001");

        // Act
        var result = await controller.Enrol(dto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Enrol_DuplicateEnrolment_ReturnsConflict()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var (_, participant, category) = SeedEventAndCategory(db);

        var controller = new EnrolmentsController(db);
        ControllerTestHelper.SetUser(controller, participant);

        var dto = new CreateEnrolmentDto(category.CategoryId, "PAY-001");

        await controller.Enrol(dto);

        // Act — try again
        var result = await controller.Enrol(dto);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Enrol_UnknownCategory_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (_, participant, _) = SeedEventAndCategory(db);

        var controller = new EnrolmentsController(db);
        ControllerTestHelper.SetUser(controller, participant);

        var result = await controller.Enrol(new CreateEnrolmentDto(9999, null));

        result.Should().BeOfType<NotFoundObjectResult>();

    }
}