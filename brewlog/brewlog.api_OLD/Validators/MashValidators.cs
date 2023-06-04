using brewlog.application.Actors;
using FluentValidation;

namespace brewlog.api.Validators
{
    public class AddPhValueValidator : AbstractValidator<BrewSessionActor.Commands.AddPhValue>
    {
        public AddPhValueValidator()
        {
            RuleFor(command => command.Ph).LessThan(10).GreaterThan(0);
        }
    }
}
