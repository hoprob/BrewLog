using Akka.Actor;
using Akka.TestKit.VsTest;
using brewlog.api.Actors;

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

            Assert.AreEqual(actual.degFarenheit, expected);
        }
    }
}