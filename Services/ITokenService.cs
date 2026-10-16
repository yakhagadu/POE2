using RaceDay.API.Models.Entities;

namespace RaceDay.API.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}