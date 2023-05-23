using Akka.Actor;
using brewlog.api.Models.Enums;

namespace brewlog.api.Actors
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
            public record GetStarterProducedCells(double gramsOfDME, double initialCells);
        }
        public class Responses
        {
            public record RequiredYeastCellsResponse(double yeastCells);
            public record YeastViabilityResponse(double percentage, double calculatedYeastCells);
            public record GetGramsOfDMEResponse(double gramsOfDME);
            public record GetStarterProducedCellsResponse(double? cellsProduced);
        }
        public YeastCalculatorActor()
        {
            Receive<Queries.GetRequiredYeastCells>(cmd =>
            {
                Sender.Tell(new Responses.RequiredYeastCellsResponse(GetYeastCellsNeeded(cmd.OriginalGravity, cmd.Volume, cmd.beerStyle)));
            });

            Receive<Queries.GetYeastViability>(cmd =>
            {
                double viability = GetYeastViabilityPercentage(cmd.YeastProductionDate);
                Sender.Tell(new Responses.YeastViabilityResponse(
                    GetYeastCellsFromViability(cmd.InitialPackageCells, viability), viability));
            });

            Receive<Queries.GetGramsOfDME>(cmd =>
            {
                Sender.Tell(new Responses.GetGramsOfDMEResponse(GetGramsOfDME(cmd.litresWater)));
            });

            Receive<Queries.GetStarterProducedCells>(cmd => Sender.Tell(new Responses.GetStarterProducedCellsResponse(GetProducedStarterCells(cmd.gramsOfDME, cmd.initialCells))));
        }

        private double GetYeastCellsNeeded(double originalGravity, double volume, BeerStyle beerStyle)
        {
            //Pitchingrate is 1.5 for Lager beers and 0.75 for Ale beers
            double pitchingRate = beerStyle == BeerStyle.Lager ? 1.5 : 0.75;
            double degreesPlato = 259 - (259 / originalGravity);
            return pitchingRate * volume * degreesPlato;
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
            return initialCells * (viabilityPercentage / 100);
        }

        private double GetGramsOfDME(double litresWater)
        {
            //Based on starter OG of 1.037.
            return Math.Round(100.91*litresWater, 1);
        }

        private double? GetProducedStarterCells(double gramsOfDME, double initialCells)
        {
            //The calculation is based on the Troester method of calculating produced yeast cells
            double? newCells = null;
            if (initialCells < 1.4 * gramsOfDME)           //TODO Wrong Values when high values...WriteTest
                newCells = 1.4 * gramsOfDME;
            else if (initialCells < 3.5 * gramsOfDME)
                newCells = (2.33 - 0.67) * initialCells / gramsOfDME;
            return newCells;
        }
    }
}
