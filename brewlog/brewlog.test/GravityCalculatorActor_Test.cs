using Akka.Actor;
using Akka.TestKit.VsTest;
using brewlog;
using brewlog.application.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace brewlog.test
{
    [TestClass]
    public class GravityCalculatorActor_Test : TestKit
    {
        IActorRef gravityCalculatorActor;

        public GravityCalculatorActor_Test()
        {
            gravityCalculatorActor = Sys.ActorOf<GravityCalculatorActor>();
        }

        [TestMethod]
        [DataRow(1.056, 1.011, 5.91)]
        [DataRow(1.080, 1.015, 8.53)]
        [DataRow(1.025, 1.007, 2.36)]
        [DataRow(5, 60, -7218.75)]
        [DataRow(1.5153, 1.2563, 33.99)]
        public async Task Calculate_Abv_Return_True(double og, double fg, double expected)
        {
            var response = await gravityCalculatorActor.Ask
                <GravityCalculatorActor.Responses.CalculatedAbvResponse>
                (new GravityCalculatorActor.Queries.GetCalculatedAbv(og, fg));

            Assert.AreEqual(expected, response.Abv);
        }

        [TestMethod]
        [DataRow(30, 1.040, 60, 1.035, 8.9)]
        [DataRow(30, 1.040, 60, 1.050, 0)]
        public async Task Get_Suggested_Boil_Sg_Adjustment_Water_Addition_Return_True(double preBoilVol, double preBoilSg, double boilMinutes, double targetOg, double expected)
        {
            var response = await gravityCalculatorActor.Ask
                <GravityCalculatorActor.Responses.GetSuggestedBoilSgAdjustmentResponse>
                (new GravityCalculatorActor.Queries.GetSuggestedBoilSgAdjustment(preBoilVol, preBoilSg,  boilMinutes, targetOg));
            Assert.AreEqual(expected, response.WaterToAdd);
        }

        [TestMethod]
        [DataRow(30, 1.040, 60, 1.050, 22)]
        [DataRow(27, 1.044, 60, 1.054, 15)]
        [DataRow(27, 1.044, 60, 1.065, 84)]
        [DataRow(30, 1.040, 60, 1.035, 0)]
        public async Task Get_Suggested_Boil_Sg_Adjustment_Extend_Boil_Minutes_Return_True(double preBoilVol, double preBoilSg, double boilMinutes, double targetOg, double expected)
        {
            var response = await gravityCalculatorActor.Ask
                <GravityCalculatorActor.Responses.GetSuggestedBoilSgAdjustmentResponse>
                (new GravityCalculatorActor.Queries.GetSuggestedBoilSgAdjustment(preBoilVol, preBoilSg, boilMinutes, targetOg));
            Assert.AreEqual(expected, response.BoilMinutesToAdd);
        }
    }
}
