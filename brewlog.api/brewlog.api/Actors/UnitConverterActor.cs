using Akka.Actor;
using Microsoft.AspNetCore.Mvc.Diagnostics;

namespace brewlog.api.Actors
{
    public class UnitConverterActor : ReceiveActor
    {
        public class Queries
        {
            public record GetDegreesCelciusInFarenheit(double degCelcius);
        }

        public class Responses
        {
            public record GetDegreesCelciusInFarenheitResponse(double degFarenheit);
        }

        public UnitConverterActor()
        {
            Receive<Queries.GetDegreesCelciusInFarenheit>(cmd => Sender.Tell(
                new Responses.GetDegreesCelciusInFarenheitResponse(ConvertCelciusToFarenheit(cmd.degCelcius))));
        }

        public double ConvertCelciusToFarenheit(double degCelcius)
        {
            return Math.Round(degCelcius * 1.8 + 32, 1);
        }
    }
}
