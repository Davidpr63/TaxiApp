using AuthenticationService.DTOmodels;
using AuthenticationService.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationService.IAuth
{
   
    public interface IAuthentication
    {
        Task RegisterUserAsync(User newUser);
        Task LoginUserAsync(LoginDTOUser loginDTOUser);
        Task UpdateUserAsync(UserDTO updateUserData);
    }
}
