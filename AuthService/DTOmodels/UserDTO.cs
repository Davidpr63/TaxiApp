using AuthService.Models;

namespace AuthService.DTOmodels
{
    public class UserDTO
    {
        public TypeOfUser? TypeOfUser { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? image { get; set; }
        public string? ImageUrl { get; set; }
    }
}
