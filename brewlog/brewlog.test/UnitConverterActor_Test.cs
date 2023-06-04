using Akka.Actor;
using Akka.TestKit.VsTest;
using brewlog.application.Actors;

namespace brewlog.test
{    
    [TestClass]
    public class UnitConverterActor_Test : TestKit
    {
        IActorRef unitConverterActor;

        public UnitConverterActor_Test()
        {
            unitConverterActor = Sys.ActorOf<UnitConverterActor>();
        }

        [TestMethod]
        [DataRow(20, 68)]
        [DataRow(2.6, 36.7)]
        [DataRow(-5, 23)]
        [DataRow(-60.5, -76.9)]
        public async Task Celcius_To_Farenheit_Test_Return_True(double degCelcius, double expected)
        {
            var actual = await unitConverterActor.Ask
                <UnitConverterActor.Responses.GetDegreesCelciusInFarenheitResponse>
                (new UnitConverterActor.Queries.GetDegreesCelciusInFarenheit(degCelcius));

            Assert.AreEqual(expected, actual.DegFarenheit);
        }

        [TestMethod]
        [DataRow(1.056, 13.805)]
        [DataRow(1.011, 2.815)]
        [DataRow(1.120, 28.062)]
        [DataRow(1.034, 8.537)]
        [DataRow(1.079, 19.105)]
        [DataRow(1.002, 0.513)]
        public async Task Convert_Specific_Gravity_To_Plato_Return_True(double specificGravity, double expected)
        {
            var actual = await unitConverterActor
                .Ask<UnitConverterActor.Responses.GetSpecificGravityInPlatoResponse>
                (new UnitConverterActor.Queries.GetSpecificGravityInPlato(specificGravity));

            Assert.AreEqual(expected, actual.Plato);
        }
    }
}