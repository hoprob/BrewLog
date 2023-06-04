using brewlog.application.Actors;
using FluentValidation;

namespace brewlog.api.Validators
{
    public class ReportPostCoolingValuesValidator : AbstractValidator<BrewSessionActor.Commands.ReportPostCoolingValues>
    {
        public ReportPostCoolingValuesValidator()
        {
            RuleFor(command => command.VolumeInFermentationVessle).NotEmpty().GreaterThan(0);
            RuleFor(command => (decimal)command.Og).GreaterThan(1).LessThan(1.200m).PrecisionScale(4, 3, false);
        }
    }
}
