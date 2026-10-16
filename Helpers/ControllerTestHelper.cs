using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Models.Entities;

namespace RaceDay.Tests.Helpers;

public static class ControllerTestHelper
{
    /// <summary>
    /// Wires up a fake HttpContext with the given user's claims
    /// so controllers using User.FindFirst(...) work in tests.
    /// </summary>
    public static void SetUser(ControllerBase controller, User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }
}