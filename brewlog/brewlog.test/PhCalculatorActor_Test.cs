using Akka.Actor;
using Akka.TestKit.VsTest;
using brewlog.application.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace brewlog.test
{
    [TestClass]
    public class PhCalculatorActor_Test : TestKit
    {
        IActorRef phCalculatorActor;

        public PhCalculatorActor_Test()
        {
            phCalculatorActor = ActorOf<PhCalculatorActor>();
        }

        [TestMethod]
        [DataRow(5.2, 5.6, 18, 12, 80, 0.2)]
        public async Task Get_PH_Lowering_Lactic_Acid_In_Ml_Return_True(double targetPh, double currentPh, double volume, double alkalinity, double lacticAcidStrength, double expected)
        {
            var result = await phCalculatorActor
                .Ask<PhCalculatorActor.Responses.GetPhLoweringAcidResponse>
                (new PhCalculatorActor.Queries.GetPhLoweringAcidByVolume(targetPh, currentPh, volume, alkalinity, lacticAcidStrength));

            Assert.AreEqual(expected, result.mlLActicAcid);
        }

        [TestMethod]
        [DataRow(5.2, 5.6, 6, 80, 6.8)]
        public async Task Get_PH_Lowering_Lactic_Acid_In_Ml_Alternative_Return_True(double targetPh, double currentPh, double grainWeigth, double lacticAcidStrength, double expected)
        {
            var result = await phCalculatorActor
                .Ask<PhCalculatorActor.Responses.GetPhLoweringAcidResponse>
                (new PhCalculatorActor.Queries.GetPhLoweringAcidByGrainWeight(targetPh, currentPh, grainWeigth, lacticAcidStrength));

            Assert.AreEqual(expected, result.mlLActicAcid);
        }
    }
}
