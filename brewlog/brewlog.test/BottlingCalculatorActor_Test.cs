using Akka.Actor;
using Akka.TestKit.VsTest;
using brewlog.api.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace brewlog.test
{
    [TestClass]
    public class BottlingCalculatorActor_Test : TestKit
    {
        IActorRef bottlingCalculatorActor;

        public BottlingCalculatorActor_Test()
        {
            bottlingCalculatorActor = Sys.ActorOf<BottlingCalculatorActor>();
        }

        [TestMethod]
        [DataRow(5, 2.4, 11.69)]
        [DataRow(9.2, 2.5, 16.8)]
        [DataRow(100, 2.5, 135.58)]
        [DataRow(-20, 2.5, -8.20)]
        [DataRow(15, 2.4, 21.09)]
        public async Task Calculate_Needed_Co2_Pressure_In_Psi_Return_True(double storageTemperature, double co2Volume, double expected)
        {
            var response = await bottlingCalculatorActor.Ask
                <BottlingCalculatorActor.Responses.GetNeededCo2PressureInPsiResponse>
                (new BottlingCalculatorActor.Queries.GetNeededCo2PressureInPsi(storageTemperature, co2Volume));

            Assert.AreEqual(expected, response.pressure);
        }
    }
}
