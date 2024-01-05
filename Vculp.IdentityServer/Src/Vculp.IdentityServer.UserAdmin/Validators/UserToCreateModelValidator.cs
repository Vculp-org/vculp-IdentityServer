using FluentValidation;
using Vculp.IdentityServer.UserAdmin.Models;

namespace Vculp.IdentityServer.UserAdmin.Validators
{
    public class UserToCreateModelValidator : AbstractValidator<UserToCreateModel>
    {
        public UserToCreateModelValidator()
        {
            RuleFor(m => m.EmailAddress)
                .NotEmpty()
                    .WithMessage("The email address is required.")
                .EmailAddress()
                    .WithMessage("The email address must be a valid email address.");
        }
    }
}
