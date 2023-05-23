using Akka.Actor;
using static brewlog.api.Actors.BrewSessionActor.Queries;

namespace brewlog.api.Actors
{
    public class PhCalculatorActor : ReceiveActor
    {
        public class Commands
        {

        }
        public class Queries
        {
            public record GetPhLoweringAcid(double targetPh, double currentPh, double volume, double alkalinity, double lacticAcidStrength);
        }
        public class Responses
        {
            public record GetPhLoweringAcidResponse(double mlLActicAcid);
        }

        private double _ppmToMvalPerLitre = 0.2;
        private double _lacticAcidMolarMass = 90.08;
        public PhCalculatorActor()
        {
            Receive<Queries.GetPhLoweringAcid>(cmd => new Responses.GetPhLoweringAcidResponse(GetPhLoweringLacticAcid(cmd)));
        }

        private double GetPhLoweringLacticAcid(Queries.GetPhLoweringAcid cmd)
        {
            var mlLacticAcid = Math.Round((_ppmToMvalPerLitre * cmd.alkalinity * Math.Pow(10, cmd.targetPh - cmd.currentPh) * 
                cmd.volume) / (_lacticAcidMolarMass * (cmd.lacticAcidStrength / 100)), 1);
            return mlLacticAcid;
        }


    }
}
