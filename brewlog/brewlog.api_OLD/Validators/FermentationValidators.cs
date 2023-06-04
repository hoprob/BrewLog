using brewlog.application.Actors;
using FluentValidation;

namespace brewlog.api.Validators
{
    public class AddFermentationValueValidator : AbstractValidator<BrewSessionActor.Commands.AddFermentationValue>
    {
        public AddFermentationValueValidator()
        {
            RuleFor(command => command.FermentationValue).NotNull();
            RuleFor(command => (decimal)command.FermentationValue.sg).GreaterThan(1).LessThan(1.200m).PrecisionScale(4, 3, false);
            RuleFor(command => command.FermentationValue.temp).GreaterThan(0);
            RuleFor(command => command.FermentationValue.ValueDateTime).NotEmpty();
        }
    }

    public class ChangeFermentationTemperatureValidator : AbstractValidator<BrewSessionActor.Commands.ChangeFermentationTemperature>
    {
        public ChangeFermentationTemperatureValidator()
        {
            RuleFor(command => command.Temperature).GreaterThan(0);
        }
    }

    public class FermentationStageCompleteValidator : AbstractValidator<BrewSessionActor.Commands.FermentationStageComplete>
    {
        public FermentationStageCompleteValidator()
        {
            RuleFor(command => (decimal)command.Fg).GreaterThan(1).LessThan(1.200m).PrecisionScale(4, 3, false);
        }
    }
}
