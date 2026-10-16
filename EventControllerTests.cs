using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.Models.DTOs;
using RaceDay.API.Models.Entities;
using RaceDay.Tests.Helpers;
using RaceDay.API.Data;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RaceDay.Tests;

public class EventControllerTests
{
    /// <summary>Seeds an organiser and a participant in the DB, returns them.</summary>
    private static (User organiserUser, User participantUser) SeedUsers(RaceDayDbContext db)
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

        db.Users.Add(organiserUser);
        db.Users.Add(participantUser);
        db.SaveChanges();
        return (organiserUser, participantUser);
    }

    [Fact]
    public async Task Create_AsOrganiser_ReturnsCreated()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var (organiser, _) = SeedUsers(db);

        var controller = new EventsController(db);
        ControllerTestHelper.SetUser(controller, organiser);

        var dto = new CreateEventDto(
            "Comrades Marathon 2026", "Running", "The ultimate human race",
            DateTime.UtcNow.AddMonths(6), "Pietermaritzburg", 25000, 1500m,
            "Route from PMB to Durban", "Open");

        // Act
        var result = await controller.Create(dto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_AsDifferentOrganiser_ReturnsForbidden()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var (organiser, _) = SeedUsers(db);

        // A second organiser creates an event
        var otherOrgUser = new User
        {
            Email = "other@test.co.za",
            PasswordHash = "x",
            FirstName = "Other",
            LastName = "Org",
            Role = "Organiser",
            Organiser = new EventOrganiser { OrganisationName = "Other Org", ContactPhone = "x" }
        };
        db.Users.Add(otherOrgUser);
        db.SaveChanges();

        var otherEvent = new Event
        {
            OrganiserId = otherOrgUser.Organiser!.OrganiserId,
            EventName = "Other Event",
            EventType = "Running",
            DateTime = DateTime.UtcNow.AddMonths(2),
            Location = "Cape Town",
            MaxParticipants = 100
        };
        db.Events.Add(otherEvent);
        db.SaveChanges();

        // Original organiser tries to update the other's event
        var controller = new EventsController(db);
        ControllerTestHelper.SetUser(controller, organiser);

        var dto = new UpdateEventDto("Hacked Name", null, null, null, null, null, null, null, null);

        // Act
        var result = await controller.Update(otherEvent.EventId, dto);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyNonArchivedEvents()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var (organiser, _) = SeedUsers(db);

        db.Events.Add(new Event
        {
            OrganiserId = organiser.Organiser!.OrganiserId,
            EventName = "Active Event",
            EventType = "Running",
            DateTime = DateTime.UtcNow.AddMonths(1),
            Location = "PMB",
            MaxParticipants = 100,
            Status = "Open"
        });
        db.Events.Add(new Event
        {
            OrganiserId = organiser.Organiser!.OrganiserId,
            EventName = "Archived Event",
            EventType = "Walking",
            DateTime = DateTime.UtcNow.AddMonths(1),
            Location = "PMB",
            MaxParticipants = 100,
            IsArchived = true
        });
        db.SaveChanges();

        var controller = new EventsController(db);

        // Act
        var result = await controller.GetAll(null, null);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var events = ok.Value.Should().BeAssignableTo<IEnumerable<EventResponseDto>>().Subject;
        events.Should().HaveCount(1);
        events.First().EventName.Should().Be("Active Event");
    }
}