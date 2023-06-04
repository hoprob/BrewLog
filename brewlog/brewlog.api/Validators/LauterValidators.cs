using brewlog.application.Actors;
using FluentValidation;

namespace brewlog.api.Validators
{
    public class AddTotalWaterInLauterValidator : AbstractValidator<BrewSessionActor.Commands.AddTotalWaterInLauter>
    {
        public AddTotalWaterInLauterValidator()
        {
            RuleFor(command => command.LitresWater).GreaterThan(0);
        }
    }
}
