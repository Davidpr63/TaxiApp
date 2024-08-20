using ApiGatewayService.DtoMapper.IDtoMapper;
using ApiGatewayService.DTOmodels;
using ApiGatewayService.Models;
using System.Reflection.Metadata.Ecma335;

namespace ApiGatewayService.DtoMapper
{
    public class Mapper : IMapper
    {
        public User DtoUserToUser(UserDTO dto)
        {
            User user = new User()
            {
                Firstname = dto.Firstname,
                Lastname = dto.Lastname,
                Username = dto.Username,
                Email = dto.Email,
                Password = dto.Password,   
                ConfirmPassword = dto.ConfirmPassword,
                Address = dto.Address,
                DateOfBirth = dto.DateOfBirth,
                ImageUrl = dto.ImageUrl
            };
            return user;
        }

        public User DtoUserToUser_UpdateUser(UserDTO dto)
        {
            User user = new User()
            {
                RowKey = dto.UserId,
                Firstname = dto.Firstname,
                Lastname = dto.Lastname,
                Username = dto.Username,
                Email = dto.Email,
                Password = dto.Password,
                ConfirmPassword = dto.ConfirmPassword,
                Address = dto.Address,
                DateOfBirth = dto.DateOfBirth,
                ImageUrl = dto.ImageUrl
            };
            return user;
        }
    }
}
