using brewlog.application.Actors;
using FluentValidation;

namespace brewlog.api.Validators
{
    public class EnterValuesFromYeastPackageValidator : AbstractValidator<BrewSessionActor.Commands.EnterValuesForYeastPackage>
    {
        public EnterValuesFromYeastPackageValidator()
        {
            RuleFor(command => command.InitialYeastCells).GreaterThan(0).LessThan(1000);
            RuleFor(command => command.YeastProductionDate).NotEmpty();
        }
    }

    public class GetGramsOfDmeNeededValidator : AbstractValidator<BrewSessionActor.Queries.GetGramsOfDMENeeded>
    {
        public GetGramsOfDmeNeededValidator()
        {
            RuleFor(command => command.LitresWater).GreaterThan(0).LessThan(20);
        }
    }

    public class StoreYeastStartersValidator : AbstractValidator<BrewSessionActor.Commands.StoreYeastStarters>
    {
        public StoreYeastStartersValidator()
        {
            RuleForEach(command => command.YeastStarters).ChildRules(starter =>
            {
                starter.RuleFor(x => x.InitialCells).GreaterThan(0);
                starter.RuleFor(x => x.DryMaltExtract).GreaterThan(0);
                starter.RuleFor(x => x.WaterLitres).GreaterThan(0);
            });
        }
    }

    public class GetStarterProducedCellsValidator : AbstractValidator<YeastCalculatorActor.Queries.GetStarterProducedCells>
    {
        public GetStarterProducedCellsValidator()
        {
            RuleFor(query => query.initialCells).GreaterThan(0);
            RuleFor(query => query.gramsOfDME).GreaterThan(0);
        }
    }
}
