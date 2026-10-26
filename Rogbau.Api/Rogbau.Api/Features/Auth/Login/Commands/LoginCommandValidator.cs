using FluentValidation;

namespace Rogbau.Api.Features.Auth.Login.Commands
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Adres email jest wymagany")
                .EmailAddress().WithMessage("Podaj poprawny adres Email");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Hasło jest wymagane");
        }
    }
}
