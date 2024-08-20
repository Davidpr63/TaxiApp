namespace ApiGatewayService.FacebookAuth
{
    public interface IFacebookAuthService
    {
        Task<FacebookAuthResult> AuthenticateAsync(string accessToken, int numOfUser);
    }
}
