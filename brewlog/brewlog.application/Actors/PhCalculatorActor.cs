using Akka.Actor;
using static brewlog.application.Actors.BrewSessionActor.Queries;

namespace brewlog.application.Actors
{
    public class PhCalculatorActor : ReceiveActor
    {
        public class Commands
        {

        }
        public class Queries
        {
            public record GetPhLoweringAcidByVolume(double targetPh, double currentPh, double volume, double alkalinity, double lacticAcidStrength);
            public record GetPhLoweringAcidByGrainWeight(double TargetPh, double CurrentPh, double GrainWeigthKg, double lacticAcidStrength);
        }
        public class Responses
        {
            public record GetPhLoweringAcidResponse(double mlLActicAcid);
        }

        private double _ppmToMvalPerLitre = 0.2;
        private double _lacticAcidMolarMass = 90.08;
        public PhCalculatorActor()
        {
            Receive<Queries.GetPhLoweringAcidByVolume>(cmd => Sender.Tell(new Responses.GetPhLoweringAcidResponse(GetPhLoweringLacticAcid(cmd))));
            Receive<Queries.GetPhLoweringAcidByGrainWeight>(cmd => Sender.Tell(new Responses.GetPhLoweringAcidResponse(GetPhLoweringLacticAcidAltenative(cmd))));
        }

        private double GetPhLoweringLacticAcid(Queries.GetPhLoweringAcidByVolume cmd)
        {
            var mlLacticAcid = Math.Round((_ppmToMvalPerLitre * cmd.alkalinity * Math.Pow(10, cmd.targetPh - cmd.currentPh) * 
                cmd.volume) / (_lacticAcidMolarMass * (cmd.lacticAcidStrength / 100)), 1);
            return mlLacticAcid; //TODO This is not a good calculation.... use the other one...
        }

        private double GetPhLoweringLacticAcidAltenative(Queries.GetPhLoweringAcidByGrainWeight cmd)
        {
            double density = 1.2; //Density of 80% Lactic Acid.
            double lacticAcidPerMl = (cmd.lacticAcidStrength / 100) * density;
            double normalityOfAcid = (lacticAcidPerMl * 1000) / _lacticAcidMolarMass;

            double mlToAdd = cmd.GrainWeigthKg * 30.262 * (cmd.CurrentPh - cmd.TargetPh) / normalityOfAcid;

            return Math.Round(mlToAdd, 1);
        }
    }
}
