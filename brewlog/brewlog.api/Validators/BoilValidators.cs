using brewlog.application.Actors;
using FluentValidation;

namespace brewlog.api.Validators
{
    public class AddPreBoilValuesValidator : AbstractValidator<BrewSessionActor.Commands.AddPreBoilValues>
    {
        public AddPreBoilValuesValidator()
        {
            RuleFor(command => command.Liters).GreaterThan(0);
            RuleFor(command => (decimal)command.Sg).GreaterThan(1).LessThan(1.200m).PrecisionScale(4, 3, false);
        }
    }

    public class AddAdditionalBoilWaterValidator : AbstractValidator<BrewSessionActor.Commands.AddAdditionalBoilWater>
    {
        public AddAdditionalBoilWaterValidator()
        {
            RuleFor(command => command.Litres).NotEmpty();
        }
    }

    public class AddExtendedBoilTimeValidator : AbstractValidator<BrewSessionActor.Commands.AddExtendedBoilTime>
    {
        public AddExtendedBoilTimeValidator()
        {
            RuleFor(command => command.Minutes).NotEmpty();
        }
    }


}
