using Akka.Actor;
using Newtonsoft.Json;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace brewlog.api.Actors
{
    public class GravityCalculatorActor : ReceiveActor
    {
        public class Queries
        {
            public record GetCalculatedAbv(double? OriginalGravity, double? FinalGravity);
            public record GetSuggestedBoilSgAdjustment(double PreBoilVol, double PreBoilSg, double BoilMinutes, double TargetOg);
        }

        public class Responses
        {
            public record CalculatedAbvResponse(double Abv, string? ErrorMessage);
            public record GetSuggestedBoilSgAdjustmentResponse(double WaterToAdd, double BoilMinutesToAdd);
        }

        public GravityCalculatorActor()
        {
            Receive<Queries.GetCalculatedAbv>(cmd =>
            {
                string? errorMessage = null;
                if (cmd.OriginalGravity is null)
                    errorMessage = "Require values missing to calculate ABV: Original Gravity";
                if (cmd.FinalGravity is null)
                    errorMessage = errorMessage is null ? "Require values missing to calculate ABV: Final Gravity"
                    : errorMessage += ", Final Gravity";
                Sender.Tell(new Responses.CalculatedAbvResponse(
                    CalculateAbv(cmd.OriginalGravity ?? 0, cmd.FinalGravity ?? 0), errorMessage));
            });

            Receive<Queries.GetSuggestedBoilSgAdjustment>(cmd =>
            {
                //The result value is based on calculated values at the end of the boil.
                double calculatedPostBoilVol = CalculateFinalBoilVolume(cmd.PreBoilVol, cmd.BoilMinutes);
                double calculatedOg = GetCalculatedOg(cmd.PreBoilVol, cmd.PreBoilSg, calculatedPostBoilVol);
                double targetPostBoilVol = GetTargetVolumeByTargetOg(calculatedPostBoilVol, calculatedOg, cmd.TargetOg);
                if(targetPostBoilVol > calculatedPostBoilVol)
                {
                    double waterToAdd = Math.Round(targetPostBoilVol - calculatedPostBoilVol, 1);
                    Sender.Tell(new Responses.GetSuggestedBoilSgAdjustmentResponse(waterToAdd, 0));
                }
                else
                {
                    double minutesToBoil = CalculateBoilTimeToLoseVol(cmd.PreBoilVol, calculatedPostBoilVol - targetPostBoilVol);
                    Sender.Tell(new Responses.GetSuggestedBoilSgAdjustmentResponse(0, minutesToBoil));
                }
            });
        }

        private double CalculateAbv(double originalGravity, double finalGravity)
        {
            return Math.Round((originalGravity - finalGravity) * 131.25, 2);
        }

        private double CalculateFinalBoilVolume(double preBoilVolume, double boilMinutes) //TODO Test method..
        {
            double evaporationRate = 0.12; //12%
            double shrinkage = 0.04; //4%

            return preBoilVolume * (1 - evaporationRate * boilMinutes / 60) * (1 - shrinkage);
        }

        private double GetCalculatedOg(double preBoilVolume, double preBoilSg, double postBoilVol)
        {
            double sg  = (preBoilSg * 1000) - 1000;
            return (sg * preBoilVolume) / postBoilVol;
        }

        private double GetTargetVolumeByTargetOg(double currentVol, double calculatedOg, double targetOg)
        {
            return currentVol * calculatedOg / ((targetOg - 1) * 1000);
        }

        private double CalculateBoilTimeToLoseVol(double preBoilVol, double volToLose)
        {
            double volLostByMinute = (preBoilVol * 0.12) / 60; // (Volume/Evaporuptionrate) / 1hour
            double minutesToBoil = Math.Round(volToLose / volLostByMinute);
            return minutesToBoil;
        }
    }
}
