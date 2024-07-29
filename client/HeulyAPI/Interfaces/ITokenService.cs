using HeulyAPI.Models;

namespace HeulyAPI.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);

    }
}
