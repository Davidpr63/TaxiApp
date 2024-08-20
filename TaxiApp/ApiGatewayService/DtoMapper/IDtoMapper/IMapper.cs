using ApiGatewayService.DTOmodels;
using ApiGatewayService.Models;

namespace ApiGatewayService.DtoMapper.IDtoMapper
{
    public interface IMapper
    {
        User DtoUserToUser(UserDTO dto); 
        User DtoUserToUser_UpdateUser(UserDTO dto); 
    }
}
