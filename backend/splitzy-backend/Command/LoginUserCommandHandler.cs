using MediatR;
using Microsoft.EntityFrameworkCore;
using splitzy_backend.Models;

namespace splitzy_backend.Command
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, bool>
    {
        private readonly AppDbContext _context;

        public LoginUserCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.LoginUser.Email, cancellationToken);

            if (user == null) return false;

            // Simple plain-text password comparison (not secure for production)
            return user.PasswordHash == request.LoginUser.Password;
        }
    }
}
