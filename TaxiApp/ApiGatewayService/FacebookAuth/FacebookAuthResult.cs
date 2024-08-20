using ApiGatewayService.Models;

namespace ApiGatewayService.FacebookAuth
{
    public class FacebookAuthResult
    {
        public bool Success { get; set; }
        public string[] Errors { get; set; }
        public string Token { get; set; }
        public User User { get; set; }
    }
}
