using IngApp.Domain.Entities.Users;

namespace IngApp.Application.Common.Interfaces.Authentication;

public interface IJwtTokenService
{
    (string Token, DateTime Expiration) GenerateToken(User user, IEnumerable<string> permissions);
}
