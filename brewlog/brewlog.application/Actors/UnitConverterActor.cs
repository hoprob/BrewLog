using Akka.Actor;

namespace brewlog.application.Actors
{
    public class UnitConverterActor : ReceiveActor
    {
        public class Queries
        {
            public record GetDegreesCelciusInFarenheit(double DegCelcius);
            public record GetSpecificGravityInPlato(double SpecificGravity);
        }

        public class Responses
        {
            public record GetDegreesCelciusInFarenheitResponse(double DegFarenheit);
            public record GetSpecificGravityInPlatoResponse(double Plato);
        }

        public UnitConverterActor()
        {
            Receive<Queries.GetDegreesCelciusInFarenheit>(cmd => Sender.Tell(
                new Responses.GetDegreesCelciusInFarenheitResponse(ConvertCelciusToFarenheit(cmd.DegCelcius))));
            Receive<Queries.GetSpecificGravityInPlato>(cmd => Sender.Tell(
                new Responses.GetSpecificGravityInPlatoResponse(ConvertSpecificGravityToPlato(cmd.SpecificGravity))));
        }

        public double ConvertCelciusToFarenheit(double degCelcius)
        {
            return Math.Round(degCelcius * 1.8 + 32, 1);
        }

        private double ConvertSpecificGravityToPlato(double specificGravity)
        {
            double degreesPlato = (-1 * 616.868) + (1111.14 * specificGravity) - (630.272 * Math.Pow(specificGravity, 2)) + (135.997 * Math.Pow(specificGravity, 3));
            //double degreesPlato = 259 - (259 / specificGravity); //Alternative, simplier but not as accurate, formula.
            return Math.Round(degreesPlato, 3);
        }

    }
}
