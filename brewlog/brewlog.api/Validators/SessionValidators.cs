using brewlog.application.Actors;
using brewlog.domain.Models;
using brewlog.domain.Models.Enums;
using FluentValidation;

namespace brewlog.api.Validators
{
    public class AddSessionRecipeCommandValidator : AbstractValidator<BrewSessionActor.Commands.AddSessionRecipe>
    {
        public AddSessionRecipeCommandValidator()
        {
            RuleFor(command => command.Recipe.BatchName).NotEmpty();
            RuleFor(command => (decimal)command.Recipe.TargetOg).GreaterThan(1).LessThan(1.200m).PrecisionScale(4, 3, false);
            RuleFor(command => (decimal)command.Recipe.TargetFg).GreaterThan(1).LessThan(1.200m).PrecisionScale(4, 3, false);
            RuleFor(command => command.Recipe.TargetMashPh).LessThan(10).GreaterThan(0);
            RuleFor(command => command.Recipe.MashVolume).GreaterThan(0);
            RuleFor(command => command.Recipe.LauterVolume).GreaterThan(0);
            RuleFor(command => command.Recipe.TargetVolume).GreaterThan(0);
            RuleFor(command => command.Recipe.MashTime).GreaterThan(0);
            RuleFor(command => command.Recipe.BoilTime).GreaterThan(0);
            RuleFor(command => command.Recipe.BeerStyle).NotNull();
        }
    }

    public class AddLogNoteValidator : AbstractValidator<BrewSessionActor.Commands.AddLogNote>
    {
        public AddLogNoteValidator()
        {
            RuleFor(command => command.Note).NotEmpty();
        }
    }
}
