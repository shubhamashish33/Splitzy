using MediatR;
using splitzy_backend.DTOs;

namespace splitzy_backend.Command
{
    public class LoginUserCommand : IRequest<bool>
    {
        public LoginUserDTO LoginUser { get;}

        public LoginUserCommand(LoginUserDTO dto)
        {
            LoginUser = dto;
        }
    }
}
