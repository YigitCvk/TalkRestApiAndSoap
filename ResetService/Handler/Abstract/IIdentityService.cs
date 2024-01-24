using Azure;
using IdentityModel.Client;
using ResetService.Models.ViewModels;

namespace ResetService.Handler.Abstract
{
    public interface IIdentityService
    {
        Task<Response<bool>> SignIn(SigninInput signinInput);

        Task<TokenResponse> GetAccessTokenByRefreshToken();

        Task RevokeRefreshToken();
    }
}
