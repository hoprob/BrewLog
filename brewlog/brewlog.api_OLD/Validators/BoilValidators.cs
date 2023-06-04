using brewlog.application.Actors;
using FluentValidation;

namespace brewlog.api.Validators
{
   public class AddPreBoilSgValidator : AbstractValidator<BrewSessionActor.Commands.AddPreBoilSg>
    {
        public AddPreBoilSgValidator()
        {
            RuleFor(command => (decimal)command.PreBoilSg).GreaterThan(1).LessThan(1.200m).PrecisionScale(4, 3, false);
        }
    }

    public class AddBoilVolumeValidator : AbstractValidator<BrewSessionActor.Commands.AddBoilVolume>
    {
        public AddBoilVolumeValidator()
        {
            RuleFor(command => command.Litres).GreaterThan(0);
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
