using Akka.Actor;
using brewlog.application.Actors;
using Google.Protobuf;

namespace brewlog.api.Actors
{
    public class BottlingCalculatorActor : ReceiveActor
    {
        public class Queries
        {
            public record GetNeededCo2PressureInPsi(double storageTemperature, double desiredCo2Volume);
        }

        public class Responses
        {
            public record GetNeededCo2PressureInPsiResponse(double pressure);
        }

        public BottlingCalculatorActor()
        {
            ReceiveAsync<Queries.GetNeededCo2PressureInPsi>(async cmd => Sender.Tell(
                new Responses.GetNeededCo2PressureInPsiResponse(
                    await CalculateNeededCo2PressureInPsi(cmd.storageTemperature, cmd.desiredCo2Volume))));
        }

        private async Task<double> CalculateNeededCo2PressureInPsi(double temperature, double desiredCo2Volume)
        {
            var unitConvertActor = Context.ActorOf<UnitConverterActor>();

            var converterResponse = await unitConvertActor.Ask
                <UnitConverterActor.Responses.GetDegreesCelciusInFarenheitResponse>(
                new UnitConverterActor.Queries.GetDegreesCelciusInFarenheit(temperature));
            //Formula is P = -16.6999 – 0.0101059*T + 0.00116512*T^2 + 0.173354*T*V + 4.24267*V – 0.0684226*V^2
            double psiPressure = -16.6999 - 0.0101059 * converterResponse.DegFarenheit + 
                0.00116512 * Math.Pow(converterResponse.DegFarenheit, 2) + 0.173354 *
                converterResponse.DegFarenheit * desiredCo2Volume + 4.24267 * desiredCo2Volume -
                0.0684226 * Math.Pow(desiredCo2Volume, 2);

            return Math.Round(psiPressure, 2);
        }

    }
}
