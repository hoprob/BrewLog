using Akka.Actor;
using brewlog.application.Interfaces;
using brewlog.domain.Models.Enums;

namespace brewlog.application.Actors
{
    public class YeastCalculatorActor : ReceiveActor
    {
        public class Commands
        {
        
        }
        public class Queries
        {
            public record GetRequiredYeastCells(double OriginalGravity, double Volume, BeerStyle beerStyle);
            public record GetYeastViability(double InitialPackageCells, DateTimeOffset YeastProductionDate);
            public record GetGramsOfDME(double litresWater);
            public record GetStarterProducedCells(double gramsOfDME, double initialCells) : IBrewSessionValidate;
        }
        public class Responses
        {
            public record RequiredYeastCellsResponse(double yeastCells);
            public record YeastViabilityResponse(double percentage, double calculatedYeastCells);
            public record GetGramsOfDMEResponse(double gramsOfDME);
            public record GetStarterProducedCellsResponse(double cellsProduced);
        }
        public YeastCalculatorActor()
        {
            ReceiveAsync<Queries.GetRequiredYeastCells>(async cmd =>
            {
                Sender.Tell(new Responses.RequiredYeastCellsResponse(await GetYeastCellsNeeded(cmd.OriginalGravity, cmd.Volume, cmd.beerStyle)));
            });

            Receive<Queries.GetYeastViability>(cmd =>
            {
                double viability = GetYeastViabilityPercentage(cmd.YeastProductionDate);
                Sender.Tell(new Responses.YeastViabilityResponse(
                    viability, GetYeastCellsFromViability(cmd.InitialPackageCells, viability)));
            });

            Receive<Queries.GetGramsOfDME>(cmd =>
            {
                Sender.Tell(new Responses.GetGramsOfDMEResponse(GetGramsOfDME(cmd.litresWater)));
            });

            Receive<Queries.GetStarterProducedCells>(cmd => Sender.Tell(new Responses.GetStarterProducedCellsResponse(GetProducedStarterCells(cmd.gramsOfDME, cmd.initialCells))));
        }

        private async Task<double> GetYeastCellsNeeded(double originalGravity, double volume, BeerStyle beerStyle)
        {
            //Pitchingrate is 1.5 for Lager beers and 0.75 for Ale beers
            double pitchingRate = beerStyle == BeerStyle.Lager ? 1.5 : 0.75;

            var unitConverterActor = Context.ActorOf<UnitConverterActor>();
            var unitConverterResponse = await unitConverterActor
                .Ask<UnitConverterActor.Responses.GetSpecificGravityInPlatoResponse>(
                new UnitConverterActor.Queries.GetSpecificGravityInPlato(originalGravity));
            
            double billionsOfYeastCellsNeeded = Math.Round(pitchingRate * volume * unitConverterResponse.Plato);
            
            return billionsOfYeastCellsNeeded;
        }

        private double GetYeastViabilityPercentage(DateTimeOffset yeastProductionDate)
        {
            //Starting percentage 97%.
            double yeastAgeDays = (DateTimeOffset.Now - yeastProductionDate).Days;
            double percentage = Math.Round(97 * (Math.Pow(2.72, (-0.008) * yeastAgeDays)), 1);
            return percentage;
        }

        private double GetYeastCellsFromViability(double initialCells, double viabilityPercentage)
        {
            return Math.Round(initialCells * (viabilityPercentage / 100), 2);
        }

        private double GetGramsOfDME(double litresWater)
        {
            //Based on starter OG of 1.037.
            return Math.Round(100.91*litresWater, 1);
        }

        private double GetProducedStarterCells(double gramsOfDME, double initialCells)
        {
            //The calculation is based on the Troester method of calculating produced yeast cells
            double newCells = initialCells;
            if (initialCells < 1.4 * gramsOfDME)     
                newCells = 1.4 * gramsOfDME;
            else if (initialCells < 3.5 * gramsOfDME)
                //newCells = (2.33 - 0.67) * initialCells / gramsOfDME; //TODO else?? test this calculation better..
                newCells = (2.33-(initialCells/gramsOfDME*0.67))*201.82;

            return Math.Round(newCells);
        }
    }
}
