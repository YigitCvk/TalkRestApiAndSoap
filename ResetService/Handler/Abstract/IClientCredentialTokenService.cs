namespace ResetService.Handler.Abstract
{
    public interface IClientCredentialTokenService
    {
        Task<String> GetToken();
    }
}
