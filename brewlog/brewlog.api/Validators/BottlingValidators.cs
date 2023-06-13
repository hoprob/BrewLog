using brewlog.application.Actors;
using FluentValidation;

namespace brewlog.api.Validators
{
    public class AddBottlingValuesValidator : AbstractValidator<BrewSessionActor.Commands.AddBottlingValues>
    {
        public AddBottlingValuesValidator()
        {
            RuleFor(command => (decimal)command.Co2Volume).PrecisionScale(2, 1, false).GreaterThan(1).LessThan(4);
            RuleFor(command => command.Temperature).NotEmpty().InclusiveBetween(-5, 50);
        }
    }
    
}
